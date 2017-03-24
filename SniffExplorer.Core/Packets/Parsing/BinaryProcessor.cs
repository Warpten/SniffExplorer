using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SniffExplorer.Core.Packets.Parsing.Attributes;

namespace SniffExplorer.Core.Packets.Parsing
{
    public class BinaryProcessor
    {
        public uint Build { get; private set; }
        public string Locale { get; private set; }

        private Dictionary<Type, Func<PacketReader, ValueType>> _typeLoaders = new Dictionary<Type, Func<PacketReader, ValueType>>();
        private Dictionary<object, Type> _opcodeStructs = new Dictionary<object, Type>();

        public object GetOpcode(uint opcodeValue, uint direction)
        {
            var targetEnum = Assembly.GetTypes().First(type =>
            {
                if (!type.IsEnum)
                    return false;

                var targetAttribute = type.GetCustomAttributes<OpcodeAttribute>();
                return targetAttribute.Any(attr => attr.TargetBuilds.Contains(Build) && attr.Direction == direction);
            });
            return targetEnum == null ? null : Enum.ToObject(targetEnum, opcodeValue);
        }

        public Assembly Assembly { get; private set; }

        private void LoadHandlers()
        {
            Assembly = SelectAssembly();
            if (Assembly == null)
                throw new InvalidOperationException($"Unable to find an assembly that handles client build {Build}");

            _opcodeStructs.Clear();

            foreach (var type in Assembly.GetTypes())
            {
                var opcodeAttrs = type.GetCustomAttributes<PacketAttribute>().ToArray();
                if (opcodeAttrs.Length == 0)
                    continue;

                if (type.GetCustomAttributes<TargetBuildAttribute>().Any(t => t.Build == Build))
                {
                    foreach (var opcodeAttribute in opcodeAttrs)
                        _opcodeStructs[opcodeAttribute.Opcode] = type;

                    _typeLoaders[type] = ParserFactory.GeneratePacketReader(type);
                }
            }
        }

        private Assembly SelectAssembly()
        {
            var targetPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // ReSharper disable once AssignNullToNotNullAttribute
            targetPath = Path.Combine(targetPath, "Versions");
            return (from file in Directory.GetFiles(targetPath, "*.dll")
                    select Assembly.LoadFile(file) into assembly
                    let targetAttr = assembly.GetCustomAttributes<TargetBuildAttribute>()
                        where targetAttr.Any(attr => attr.Build == Build)
                    select assembly).FirstOrDefault();
        }

        public void Process(string filePath)
        {
            using (var strm = File.OpenRead(filePath))
                Process(strm);
        }

        public void Process(Stream strm)
        {
            using (var sniffStream = new BinaryReader(strm))
            {
                sniffStream.BaseStream.Position += 3 + 2;
                var snifferId = (char) sniffStream.ReadByte();
                Build = sniffStream.ReadUInt32();
                Locale = Encoding.UTF8.GetString(sniffStream.ReadBytes(4));
                sniffStream.BaseStream.Position += 40;
                var startTimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds(sniffStream.ReadUInt32());
                var startTickCount = sniffStream.ReadUInt32();
                var optDataLength = sniffStream.ReadInt32();
                var optData = sniffStream.ReadBytes(optDataLength);

                LoadHandlers();

                if (snifferId == 'S') // WSTC
                {
                    // versions 1.5 and older store human readable sniffer description string in header
                    // version 1.6 adds 3 bytes before that data, 0xFF separator, one byte for major version and one byte for minor version, expecting 0x0106 for 1.6
                    short snifferVersion;
                    if (optDataLength >= 3 && optData[0] == 0xFF)
                        snifferVersion = BitConverter.ToInt16(optData, 1);
                    else
                        snifferVersion = 0x0105;

                    if (snifferVersion >= 0x0107)
                        startTimeStamp = DateTime.FromFileTime(BitConverter.ToInt64(optData, 3));
                }

                var packetList = new List<Packet>(5000);

                while (sniffStream.BaseStream.Position < sniffStream.BaseStream.Length)
                {
                    var direction = sniffStream.ReadUInt32();
                    var connectionID = sniffStream.ReadUInt32();
                    var timeStamp = startTimeStamp.AddMilliseconds(sniffStream.ReadUInt32() - startTickCount);
                    var optionalHeaderLength = sniffStream.ReadUInt32();
                    var fullSize = sniffStream.ReadInt32() - 4;
                    sniffStream.BaseStream.Position += optionalHeaderLength;
                    var opcode = sniffStream.ReadUInt32();

                    packetList.Add(new Packet
                    {
                        Opcode = opcode,
                        TimeStamp = timeStamp,
                        ConnectionID = connectionID,
                        Direction = direction,
                        Data = sniffStream.ReadBytes(fullSize)
                    });
                }

                Console.WriteLine("Parsing {0} packets...", packetList.Count);

                Parallel.ForEach(packetList, packet =>
                {
                    using (var memoryStream = new MemoryStream(packet.Data, false))
                    using (var packetReader = new PacketReader(memoryStream, packet.Data.Length))
                    {
                        var opcodeName = GetOpcode(packet.Opcode, packet.Direction);
                        Type targetType;
                        if (!_opcodeStructs.TryGetValue(opcodeName, out targetType))
                            return;

                        Func<PacketReader, ValueType> reader;
                        if (!_typeLoaders.TryGetValue(targetType, out reader))
                            return; //! TODO: Assert here

                        OnPacketParsed?.Invoke(string.Intern(opcodeName.ToString()), reader(packetReader), packet.ConnectionID,
                            packet.TimeStamp);

                        if (memoryStream.Position == memoryStream.Length)
                            return;

                        Console.WriteLine("Incomplete parsing of {0} ({1} bytes read, {2} remaining)", opcodeName,
                            memoryStream.Position, memoryStream.Length - memoryStream.Position);

                        //! TODO: Move to sub-dlls
#if DEBUG
                        Console.WriteLine(
                            @"|-------------------------------------------------|---------------------------------|");
                        Console.WriteLine(
                            @"| 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | 0 1 2 3 4 5 6 7 8 9 A B C D E F |");
                        Console.WriteLine(
                            @"|-------------------------------------------------|---------------------------------|");

                        for (var i = memoryStream.Position; i < memoryStream.Length; i += 16)
                        {
                            var hexBuffer = new StringBuilder();
                            var asciiBuffer = new StringBuilder();
                            for (var j = 0; j < 16; ++j)
                            {
                                if (i + j < memoryStream.Length)
                                {
                                    var value = packetReader.ReadByte();
                                    hexBuffer.Append($"{value:X2} ");
                                    if (value >= 32 && value <= 127)
                                        asciiBuffer.Append($"{(char) value} ");
                                    else
                                        asciiBuffer.Append(". ");
                                }
                                else
                                {
                                    hexBuffer.Append("   ");
                                    asciiBuffer.Append("  ");
                                }
                            }

                            Console.WriteLine($"| {hexBuffer}| {asciiBuffer} |");
                        }

                        Console.WriteLine(
                            @"|-------------------------------------------------|---------------------------------|");
#endif // DEBUG
                    }
                });
            }
        }

        public event Action<string, ValueType, uint, DateTime> OnPacketParsed;

        public struct Packet
        {
            public uint Opcode { get; set; }
            public uint ConnectionID { get; set; }
            public DateTime TimeStamp { get; set; }
            public byte[] Data { get; set; }
            public uint Direction { get; set; }
        }
    }
}
