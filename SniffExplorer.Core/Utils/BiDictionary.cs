using System;
using System.Collections;
using System.Collections.Generic;

namespace SniffExplorer.Core.Utils
{
    /// <summary>
    /// A dumb and horrible implementation of a bidictionary.
    ///
    /// It allows for enumerable initialization ( { { ..., ... }, ... } ) By pretending to be a simple dictionary.
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    public sealed class BiDictionary<TFirst, TSecond> : IDictionary<TFirst, TSecond>
    {
        IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        public TSecond this[TFirst key] { get => firstToSecond[key]; set => firstToSecond[key] = value; }

        public ICollection<TFirst> Keys => firstToSecond.Keys;

        public ICollection<TSecond> Values => firstToSecond.Values;

        public int Count => firstToSecond.Count;

        public bool IsReadOnly => firstToSecond.IsReadOnly;

        public void Add(TFirst first, TSecond second)
        {
            if (firstToSecond.ContainsKey(first) ||
                secondToFirst.ContainsKey(second))
            {
                throw new ArgumentException("Duplicate first or second");
            }
            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
        }

        public void Add(KeyValuePair<TFirst, TSecond> item)
        {
            firstToSecond.Add(item);
        }

        public void Clear()
        {
            firstToSecond.Clear();
        }

        public bool Contains(KeyValuePair<TFirst, TSecond> item)
        {
            return firstToSecond.Contains(item);
        }

        public bool ContainsKey(TFirst key)
        {
            return firstToSecond.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TFirst, TSecond>[] array, int arrayIndex)
        {
            firstToSecond.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TFirst, TSecond>> GetEnumerator()
        {
            return firstToSecond.GetEnumerator();
        }

        public bool Remove(TFirst key)
        {
            return firstToSecond.Remove(key);
        }

        public bool Remove(KeyValuePair<TFirst, TSecond> item)
        {
            return firstToSecond.Remove(item);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return firstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return secondToFirst.TryGetValue(second, out first);
        }

        public bool TryGetValue(TFirst key, out TSecond value)
        {
            return firstToSecond.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return firstToSecond.GetEnumerator();
        }
    }
}
