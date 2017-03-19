using System;
using System.Collections.Generic;

namespace SniffExplorer.Packets.Parsing
{
    public static class TypeReadersStore<T>
    {
        private static Dictionary<Type, T> _values = new Dictionary<Type, T>();

        public static void Store(Type key, T value)
        {
            _values[key] = value;
        }

        public static bool TryGetValue(Type key, out T value)
        {
            return _values.TryGetValue(key, out value);
        }

        public static bool ContainsKey(Type key) => _values.ContainsKey(key);
    }
}
