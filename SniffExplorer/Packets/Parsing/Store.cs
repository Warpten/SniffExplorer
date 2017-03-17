using System;
using System.Collections.Generic;
using SniffExplorer.Enums;
using SniffExplorer.Utils;

namespace SniffExplorer.Packets.Parsing
{
    public static class Store
    {
        public class Record
        {
            public Either<OpcodeClient, OpcodeServer> Opcode { get; set; }
            public ValueType Packet { get; set; }
            
            public DateTime TimeStamp { get; set; }
            public uint ConnectionID { get; set; }
        }

        public static List<Record> Opcodes { get; } =
            new List<Record>();

        public static void Insert(OpcodeClient opcode, ValueType instance, uint connectionId, DateTime timeStamp)
        {
            Opcodes.Add(new Record {
                Opcode = new Either<OpcodeClient, OpcodeServer>(opcode),
                Packet = instance,

                ConnectionID = connectionId,
                TimeStamp = timeStamp
            });
        }

        public static void Insert(OpcodeServer opcode, ValueType instance, uint connectionId, DateTime timeStamp)
        {
            Opcodes.Add(new Record
            {
                Opcode = new Either<OpcodeClient, OpcodeServer>(opcode),
                Packet = instance,

                ConnectionID = connectionId,
                TimeStamp = timeStamp
            });
        }
    }
}
