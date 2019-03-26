// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    // Extensions.Array
    public static partial class Extensions
    {
        /// <summary>
        /// Linq Concat replacement for byte arrays.
        /// </summary>
        public static byte[] Concat(this byte[] self, byte[] other)
        {
            if ((object)self == null) { return other; }
            if ((object)other == null) { return self; }
            byte[] result = new byte[self.Length + other.Length];
            System.Buffer.BlockCopy(self, 0, result, 0, self.Length);
            System.Buffer.BlockCopy(other, 0, result, self.Length, other.Length);
            return result;
        }

        /// <summary>
        /// Specialized SubArray extension method for byte arrays.
        /// </summary>
        public static byte[] SubArray(this byte[] self, int offset, int count)
        {
            if ((object)self == null) { return null; }
            byte[] result = new byte[count];
            System.Buffer.BlockCopy(self, offset, result, 0, count);
            return result;
        }

        /// <summary>
        /// Returns a new subarray that delimits the specified range of the
        /// elements in the specified source array.
        /// </summary>
        public static T[] SubArray<T>(this T[] self, int offset, int count)
        {
            if ((object)self == null) { return null; }
            T[] result = new T[count];
            Array.Copy(self, offset, result, 0, count);
            return result;
        }
    }
}
