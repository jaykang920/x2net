// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2net
{
    // Extensions.Equals
    public static partial class Extensions
    {
        public static bool EqualsEx(this bool self, bool other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this byte self, byte other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this sbyte self, sbyte other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this short self, short other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this int self, int other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this long self, long other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this float self, float other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this double self, double other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this string self, string other)
        {
            return (self == other);
        }

        public static bool EqualsEx(this DateTime self, DateTime other)
        {
            return (self == other);
        }

        /// <summary>
        /// Tests for the sequence equality of the specified byte arrays.
        /// </summary>
        public static bool EqualsEx(this byte[] self, byte[] other)
        {
            if (ReferenceEquals(self, other)) { return true; }
            if (ReferenceEquals(self, null) || ReferenceEquals(other, null)) { return false; }
            int length = self.Length;
            if (length != other.Length) { return false; }
            for (int i = 0; i < length; ++i)
            {
                if (self[i] != other[i]) { return false; }
            }
            return true;
        }

        public static bool EqualsEx<T>(this T self, T other) where T : class
        {
            if (ReferenceEquals(self, other)) { return true; }
            if (ReferenceEquals(self, null) || ReferenceEquals(other, null)) { return false; }
            return self.Equals(other);
        }

        public static bool EqualsEx<T>(this List<T> self, List<T> other)
        {
            if (ReferenceEquals(self, other)) { return true; }
            if (ReferenceEquals(self, null) || ReferenceEquals(other, null)) { return false; }
            int count = self.Count;
            if (count != other.Count) { return false; }
            bool isValueType = typeof(T).IsValueType;
            for (int i = 0; i < count; ++i)
            {
                T mine = self[i];
                T others = other[i];
                if (!isValueType)
                {
                    if (ReferenceEquals(mine, others)) { return true; }
                    if (ReferenceEquals(mine, null) || ReferenceEquals(others, null)) { return false; }
                }
#if NET40
                if (!EqualsEx((dynamic)mine, (dynamic)others))
#else
                if (!EqualityComparer<T>.Default.Equals(mine, others))
#endif
                {
                    return false;
                }
            }
            return true;
        }

        public static bool EqualsEx<T, U>(this Dictionary<T, U> self, Dictionary<T, U> other)
        {
            if (ReferenceEquals(self, other)) { return true; }
            if (ReferenceEquals(self, null) || ReferenceEquals(other, null)) { return false; }
            int count = self.Count;
            if (count != other.Count) { return false; }
            bool isValueValueType = typeof(U).IsValueType;
            foreach (var pair in self)
            {
                T myKey = pair.Key;
                U myValue = pair.Value;
                U otherValue;
                if (!other.TryGetValue(myKey, out otherValue)) { return false; }
                if (!isValueValueType)
                {
                    if (ReferenceEquals(myValue, otherValue)) { return true; }
                    if (ReferenceEquals(myValue, null) || ReferenceEquals(otherValue, null)) { return false; }
                }
#if NET40
                if (!EqualsEx((dynamic)myValue, (dynamic)otherValue))
#else
                if (!EqualityComparer<U>.Default.Equals(myValue, otherValue))
#endif
                {
                    return false;
                }
            }
            return true;
        }

#if NET40
        private static bool EqualsEx(dynamic x, dynamic y)
        {
            return x.EqualsEx(y);
        }
#endif
    }
}
