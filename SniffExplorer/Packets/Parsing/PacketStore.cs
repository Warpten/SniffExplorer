using System;
using System.Collections.Generic;
using System.Linq;
using SniffExplorer.Enums;
using SniffExplorer.Utils;

namespace SniffExplorer.Packets.Parsing
{
    public static class PacketStore
    {
        public class Record
        {
            public Either<OpcodeClient, OpcodeServer> Opcode { get; set; }
            public ValueType Packet { get; set; }
            
            public DateTime TimeStamp { get; set; }
            public uint ConnectionID { get; set; }
        }

        private static List<Record> Opcodes { get; } =
            new List<Record>();

        public static void Insert(Either<OpcodeClient, OpcodeServer> opcode, ValueType instance, uint connectionId, DateTime timeStamp)
        {
            Opcodes.Add(new Record {
                Opcode = opcode,
                Packet = instance,

                ConnectionID = connectionId,
                TimeStamp = timeStamp
            });
        }

        public static IEnumerable<Record> GetPackets(Either<OpcodeClient, OpcodeServer> key) =>
            Opcodes.Where(r => r.Opcode == key);

        public static IEnumerator<Record> GetIterator() =>
            Opcodes.GetEnumerator();

        public static int Count => Opcodes.Count;
    }
}
