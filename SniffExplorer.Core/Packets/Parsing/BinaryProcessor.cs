using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SniffExplorer.Core.Attributes;

namespace SniffExplorer.Core.Packets.Parsing
{
    public class BinaryProcessor
    {
        public uint Build { get; private set; }
        public string Locale { get; private set; }
        public int Count { get; private set; }

        public IOpcodeProvider OpcodeProvider { get; private set; }

        private Dictionary<Type, Func<PacketReader, ValueType>> _typeLoaders = new Dictionary<Type, Func<PacketReader, ValueType>>();
        private Dictionary<Opcodes, Type> _opcodeStructs = new Dictionary<Opcodes, Type>();

        public Assembly Assembly { get; private set; }

        private void LoadHandlers()
        {
            Assembly = SelectAssembly();
            if (Assembly == null)
                throw new InvalidOperationException($"Unable to find an assembly that handles client build {Build}");

            _opcodeStructs.Clear();

            foreach (var type in Assembly.GetTypes())
            {
                if (type.GetCustomAttributes<TargetBuildAttribute>().Any(t => t.Build == Build))
                {
                    var opcodeAttrs = type.GetCustomAttributes<PacketAttribute>().ToArray();
                    if (opcodeAttrs.Length == 0)
                        continue;

                    foreach (var opcodeAttribute in opcodeAttrs)
                        _opcodeStructs[opcodeAttribute.Opcode] = type;

                    _typeLoaders[type] = ParserFactory.GeneratePacketReader(type);
                }
                else if (typeof(IOpcodeProvider).IsAssignableFrom(type))
                {
                    OpcodeProvider = (IOpcodeProvider)Activator.CreateInstance(type);
                }
            }

            if (OpcodeProvider == null)
                throw new InvalidProgramException("Unable to find an implementation of IOpcodeProvider to instanciate!");
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
                    var direction = (PacketDirection)sniffStream.ReadUInt32();
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

                Count = packetList.Count;

                OnSniffPrepared?.Invoke();

                Parallel.ForEach(packetList, packet =>
                {
                    using (var memoryStream = new MemoryStream(packet.Data, false))
                    using (var packetReader = new PacketReader(memoryStream, packet.Data.Length))
                    {
                        var opcodeName = OpcodeProvider.ValueToOpcode(packet.Opcode, packet.Direction);
                        if (!_opcodeStructs.TryGetValue(opcodeName, out Type targetType))
                            return;

                        if (!_typeLoaders.TryGetValue(targetType, out var reader))
                            return; //! TODO: Assert here when we successfully implemented every structure (aka never)

                        OnPacketParsed?.Invoke(string.Intern(opcodeName.ToString()), reader(packetReader), packet.ConnectionID,
                            packet.TimeStamp);

                        packet.Data = null;

                        if (memoryStream.Position == memoryStream.Length)
                            return;

                        Console.WriteLine("Incomplete parsing of {0} ({1} bytes read, {2} remaining)", opcodeName,
                            memoryStream.Position, memoryStream.Length - memoryStream.Position);
                    }
                });

                GC.Collect();
            }
        }

        public event Action OnSniffPrepared;
        public event Action<string, ValueType, uint, DateTime> OnPacketParsed;

        public struct Packet
        {
            public uint Opcode { get; set; }
            public uint ConnectionID { get; set; }
            public DateTime TimeStamp { get; set; }
            public byte[] Data { get; set; }
            public PacketDirection Direction { get; set; }
        }
    }
}
