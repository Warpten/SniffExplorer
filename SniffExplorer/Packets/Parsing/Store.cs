using System.Collections.Generic;

namespace SniffExplorer.Packets.Parsing
{
    public static class Store<T> where T : struct, IPacketStruct
    {
        public static List<T> Values { get; } = new List<T>(100);

        public static void Insert(T instance)
        {
            Values.Add(instance);
        }
    }
}
