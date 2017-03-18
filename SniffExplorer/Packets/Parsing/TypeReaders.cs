using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SniffExplorer.Packets.Parsing
{
    public static class TypeReadersStore<T>
    {
        private static Dictionary<Type, T> _values = new Dictionary<Type, T>();

        public static void Store(Type key, T value)
        {
            _values[key] = value;
        }

        public static T Get(Type key)
        {
            return _values[key];
        }

        public static bool ContainsKey(Type key) => _values.ContainsKey(key);
    }
}
