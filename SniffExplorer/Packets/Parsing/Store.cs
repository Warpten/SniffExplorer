using System.Collections.Generic;
using SniffExplorer.Enums;
using SniffExplorer.Utils;

namespace SniffExplorer.Packets.Parsing
{
    public static class Store
    {
        public static Dictionary<Either<OpcodeClient, OpcodeServer>, IPacketStruct> Opcodes { get; } =
            new Dictionary<Either<OpcodeClient, OpcodeServer>, IPacketStruct>();

        public static void Insert(OpcodeClient opcode, IPacketStruct instance)
        {
            Opcodes[new Either<OpcodeClient, OpcodeServer>(opcode)] = instance;
        }

        public static void Insert(OpcodeServer opcode, IPacketStruct instance)
        {
            Opcodes[new Either<OpcodeClient, OpcodeServer>(opcode)] = instance;
        }
    }
}
