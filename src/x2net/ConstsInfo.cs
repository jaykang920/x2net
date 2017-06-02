// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// Provides the common housekeeping methods for static constants.
    /// </summary>
    public sealed class ConstsInfo<T>
    {
        private SortedList<string, T> map;

        public ConstsInfo()
        {
            map = new SortedList<string, T>();
        }

        public void Add(string name, T value)
        {
            map.Add(name, value);
        }

        public bool ContainsName(string name)
        {
            return map.ContainsKey(name);
        }

        public bool ContainsValue(T value)
        {
            return map.ContainsValue(value);
        }

        public string GetName(T value)
        {
            IList<T> values = map.Values;
            for (int i = 0, count = values.Count; i < count; ++i)
            {
                if (values[i].Equals(value))
                {
                    return map.Keys[i];
                }
            }
            return null;
        }

        public T Parse(string name)
        {
            return map[name];
        }

        public bool TryParse(string name, out T result)
        {
            return map.TryGetValue(name, out result);
        }
    }
}
