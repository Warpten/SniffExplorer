using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SniffExplorer.Enums;

namespace SniffExplorer.Packets.Parsing
{
    public static class BinaryProcessor
    {
        public static uint Build { get; private set; }
        public static string Locale { get; private set; }

        private static Dictionary<OpcodeClient, Type> _clientOpcodeStructs = new Dictionary<OpcodeClient, Type>();
        private static Dictionary<OpcodeServer, Type> _serverOpcodeStructs = new Dictionary<OpcodeServer, Type>();

        static BinaryProcessor()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsClass)
                    continue;

                var clientOpcodeAttrs = type.GetCustomAttributes<ClientPacketAttribute>();
                foreach (var opcodeAttribute in clientOpcodeAttrs)
                    _clientOpcodeStructs[opcodeAttribute.Opcode] = type;

                var serverOpcodeAttrs = type.GetCustomAttributes<ServerPacketAttribute>();
                foreach (var opcodeAttribute in serverOpcodeAttrs)
                    _serverOpcodeStructs[opcodeAttribute.Opcode] = type;
            }
        }

        public static void Process(string filePath)
        {
            using (var strm = File.OpenRead(filePath))
                Process(strm);
        }

        public static void Process(Stream strm)
        {
            using (var sniffStream = new BinaryReader(strm))
            {
                sniffStream.BaseStream.Position += 3 + 2 + 1;
                Build = sniffStream.ReadUInt32();
                Locale = System.Text.Encoding.UTF8.GetString(sniffStream.ReadBytes(4));
                sniffStream.BaseStream.Position += 40 + 4 + 4 + 4;

                while (sniffStream.BaseStream.Position < sniffStream.BaseStream.Length)
                {
                    var direction = sniffStream.ReadUInt32();
                    var connectionID = sniffStream.ReadUInt32();
                    var timeStamp = sniffStream.ReadUInt32();
                    var optionalHeaderLength = sniffStream.ReadUInt32();
                    var fullSize = sniffStream.ReadUInt32() - 4;
                    sniffStream.BaseStream.Position += optionalHeaderLength;
                    var opcode = sniffStream.ReadUInt32();

                    Type targetType;
                    switch (direction)
                    {
                        case 0x47534D43u: // CMSG
                            targetType = _clientOpcodeStructs[(OpcodeClient) opcode];
                            break;
                        case 0x47534D53u: // SMSG
                            targetType = _serverOpcodeStructs[(OpcodeServer) opcode];
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Yuck.
                    Activator.CreateInstance(typeof (Packet<>).MakeGenericType(targetType), sniffStream, timeStamp,
                        fullSize, connectionID);
                }
            }
        }
    }
}
