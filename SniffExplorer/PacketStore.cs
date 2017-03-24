using System;
using System.Collections.Generic;
using System.Linq;

namespace SniffExplorer
{
    public static class PacketStore
    {
        public class Record
        {
            public string Opcode { get; set; }
            public ValueType Packet { get; set; }

            public DateTime TimeStamp { get; set; }
            public uint ConnectionID { get; set; }

            public override string ToString()
            {
                return $"{TimeStamp:dd/MM/yy HH:mm:ss.ffffff}";
            }
        }

        private static Dictionary<string, List<Record>> Opcodes { get; } =
            new Dictionary<string, List<Record>>();

        public static IEnumerable<string> GetAvailablePackets() => Opcodes.Keys;

        public static IEnumerable<Record> GetPackets(IEnumerable<string> opcodeNames)
        {
            lock (Opcodes)
                return Opcodes.Where(kv => opcodeNames.Contains(kv.Key)).SelectMany(k => k.Value).OrderBy(v => v.TimeStamp);
        }

        public static void Insert(string opcode, ValueType instance, uint connectionId, DateTime timeStamp)
        {
            var copy = string.Intern(opcode);

            lock (Opcodes)
            {
                List<Record> container;
                if (!Opcodes.TryGetValue(copy, out container))
                    container = Opcodes[copy] = new List<Record>();

                container.Add(new Record
                {
                    Opcode = copy,
                    Packet = instance,

                    ConnectionID = connectionId,
                    TimeStamp = timeStamp
                });
            }
        }
    }
}
