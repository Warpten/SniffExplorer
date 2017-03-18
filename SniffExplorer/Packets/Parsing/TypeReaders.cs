using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SniffExplorer.Packets.Parsing
{
    public static class PacketTypeReadersStore
    {
        private static Dictionary<Type, Func<PacketReader, ValueType>> _values = new Dictionary<Type, Func<PacketReader, ValueType>>();

        public static void Store(Type key, Func<PacketReader, ValueType> value)
        {
            _values[key] = value;
        }

        public static Func<PacketReader, ValueType> Get(Type key)
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
