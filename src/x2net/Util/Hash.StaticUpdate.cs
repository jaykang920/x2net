// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    // Hash.StaticUpdate
    public partial struct Hash
    {
        public static int Update(int seed, bool value)
        {
            return ((seed << 5) + seed) ^ (value ? 1 : 0);
        }

        public static int Update(int seed, sbyte value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, byte value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, short value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, ushort value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, int value)
        {
            return ((seed << 5) + seed) ^ value;
        }

        public static int Update(int seed, uint value)
        {
            return ((seed << 5) + seed) ^ (int)value;
        }

        public static int Update(int seed, long value)
        {
            return ((seed << 5) + seed) ^ (int)(value ^ (value >> 32));
        }

        public static int Update(int seed, ulong value)
        {
            return ((seed << 5) + seed) ^ (int)(value ^ (value >> 32));
        }

        public static int Update(int seed, float value)
        {
            return Update(seed, System.BitConverter.DoubleToInt64Bits((double)value));
        }

        public static int Update(int seed, double value)
        {
            return Update(seed, System.BitConverter.DoubleToInt64Bits(value));
        }

        public static int Update(int seed, string value)
        {
            return ((seed << 5) + seed) ^
                ((object)value != null ? value.GetHashCode() : 0);
        }

        public static int Update(int seed, DateTime value)
        {
            return Update(seed, value.Ticks);
        }

        public static int Update(int seed, byte[] value)
        {
            int result = seed;
            for (int i = 0; i < value.Length; ++i)
            {
                result = Update(result, value[i]);
            }
            return result;
        }

        public static int Update<T>(int seed, List<T> value)
        {
            int result = seed;
            for (int i = 0; i < value.Count; ++i)
            {
                result = Update(result, value[i]);
            }
            return result;
        }

        public static int Update<T>(int seed, T value)
        {
            return ((seed << 5) + seed) ^ 
                ((object)value != null ? value.GetHashCode() : 0);
        }
    }
}
