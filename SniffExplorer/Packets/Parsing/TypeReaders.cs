using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Packets.Parsing
{
    public static class PacketTypeReadersStore<T>
    {
        private static Dictionary<Type, Func<PacketReader, T>> _values = new Dictionary<Type, Func<PacketReader, T>>();

        public static void Store(Type key, Func<PacketReader, T> value)
        {
            _values[key] = value;
        }

        public static Func<PacketReader, T> Get(Type key)
        {
            return _values[key];
        }

        public static bool ContainsKey(Type key) => _values.ContainsKey(key);
    }

    public static class TypeReadersStore
    {
        private static Dictionary<Type, BlockExpression> _values = new Dictionary<Type, BlockExpression>();

        public static void Store(Type key, BlockExpression value)
        {
            _values[key] = value;
        }

        public static BlockExpression Get(Type key)
        {
            return _values[key];
        }

        public static bool ContainsKey(Type key) => _values.ContainsKey(key);
    }
}
