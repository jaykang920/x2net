// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2net
{
    // Extensions.Equals
    public static partial class Extensions
    {
        /// <summary>
        /// Tests for the sequence equality of the specified byte arrays.
        /// </summary>
        public static bool EqualsEx(this byte[] self, byte[] other)
        {
            if ((object)self == null && (object)other == null) { return true; }
            if ((object)self == null || (object)other == null) { return false; }
            if (self.Length != other.Length) { return false; }
            for (int i = 0, length = self.Length; i < length; ++i)
            {
                if (self[i] != other[i]) { return false; }
            }
            return true;
        }

        public static bool EqualsEx<T>(this IList<T> self, IList<T> other)
        {
            if (ReferenceEquals(self, other))
            {
                return true;
            }
            if ((object)self == null || (object)other == null)
            {
                return false;
            }
            int count = self.Count;
            if (count != other.Count)
            {
                return false;
            }
            for (int i = 0; i < count; ++i)
            {
                T mine = self[i];
                T others = other[i];
                if (mine is Cell)
                {
                    if (!ReferenceEquals(mine, others))
                    {
                        if (mine == null || others == null)
                        {
                            return false;
                        }
                        if (!mine.Equals(others))
                        {
                            return false;
                        }
                    }
                }
                else if (!EqualityComparer<T>.Default.Equals(mine, others))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Equivalent(this byte[] self, byte[] other)
        {
            return self.EqualsEx(other);
        }

        public static bool Equivalent<T>(this IList<T> self, IList<T> other)
        {
            return self.EqualsEx(other);
        }
    }
}
