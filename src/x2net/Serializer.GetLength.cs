// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    // Serializer.GetLength
    public sealed partial class Serializer
    {
        // Overloaded GetLength for primitive types

        /// <summary>
        /// Gets the number of bytes required to encode the specified boolean
        /// value.
        /// </summary>
        public static int GetLength(bool value) { return 1; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified single
        /// byte.
        /// </summary>
        public static int GetLength(byte value) { return 1; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 8-bit
        /// signed integer.
        /// </summary>
        public static int GetLength(sbyte value) { return 1; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 16-bit
        /// signed integer.
        /// </summary>
        public static int GetLength(short value) { return 2; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 32-bit
        /// signed integer.
        /// </summary>
        public static int GetLength(int value)
        {
            return GetLengthVariable((uint)((value << 1) ^ (value >> 31)));
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 32-bit
        /// non-negative integer.
        /// </summary>
        public static int GetLengthNonnegative(int value)
        {
            if (value < 0) { throw new ArgumentOutOfRangeException(); }
            return GetLengthVariable((uint)value);
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 64-bit
        /// signed integer.
        /// </summary>
        public static int GetLength(long value)
        {
            return GetLengthVariable((ulong)((value << 1) ^ (value >> 63)));
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 32-bit
        /// floating-point number.
        /// </summary>
        public static int GetLength(float value) { return 4; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 64-bit
        /// floating-point number.
        /// </summary>
        public static int GetLength(double value) { return 8; }

        /// <summary>
        /// Gets the number of bytes required to encode the specified text
        /// string.
        /// </summary>
        public static int GetLength(string value)
        {
            int length = GetLengthUTF8(value);
            return GetLengthNonnegative(length) + length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified text
        /// string with UTF-8 encoding.
        /// </summary>
        private static int GetLengthUTF8(string value)
        {
            int length = 0;
            if (!ReferenceEquals(value, null))
            {
                for (int i = 0, count = value.Length; i < count; ++i)
                {
                    char c = value[i];

                    if ((c & 0xff80) == 0) { ++length; }
                    else if ((c & 0xf800) != 0) { length += 3; }
                    else { length += 2; }
                }
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified datetime
        /// value.
        /// </summary>
        public static int GetLength(DateTime value) { return 8; }

        // Overloaded GetLength for composite types

        /// <summary>
        /// Gets the number of bytes required to encode the specified array of
        /// bytes.
        /// </summary>
        public static int GetLength(byte[] value)
        {
            int length = ReferenceEquals(value, null) ? 0 : value.Length;
            return GetLengthNonnegative(length) + length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of boolean values.
        /// </summary>
        public static int GetLength(List<bool> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of byte values.
        /// </summary>
        public static int GetLength(List<byte> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of 8-bit signed integers.
        /// </summary>
        public static int GetLength(List<sbyte> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of 16-bit signed integers.
        /// </summary>
        public static int GetLength(List<short> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of Int32 values.
        /// </summary>
        public static int GetLength(List<int> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of Int64 values.
        /// </summary>
        public static int GetLength(List<long> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of 32-bit floating-point values.
        /// </summary>
        public static int GetLength(List<float> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of 64-bit floating-point values.
        /// </summary>
        public static int GetLength(List<double> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of text strings.
        /// </summary>
        public static int GetLength(List<string> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of DateTime values.
        /// </summary>
        public static int GetLength(List<DateTime> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of Int32 lists.
        /// </summary>
        public static int GetLength(List<List<int>> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of string lists.
        /// </summary>
        public static int GetLength(List<List<string>> value)
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified ordered
        /// list of Cell-derived objects.
        /// </summary>
        public static int GetLength<T>(List<T> value) where T : Cell
        {
            int count = ReferenceEquals(value, null) ? 0 : value.Count;
            int length = GetLengthNonnegative(count);
            for (int i = 0; i < count; ++i)
            {
                length += GetLength(value[i]);
            }
            return length;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified
        /// Cell-derived object.
        /// </summary>
        public static int GetLength<T>(T value) where T : Cell
        {
            bool isNull = ReferenceEquals(value, null);
            bool partial = false;
            Type type = typeof(T);
            if (!isNull)
            {
                if (type != value.GetType()) { partial = true; }
            }
            bool flag = true;
            int length = isNull ? 0 :
                (partial ? value.GetLength(type, ref flag) : value.GetLength());
            return GetLengthNonnegative(length) + length;
        }

        // GetLength helper methods

        /// <summary>
        /// Gets the number of bytes required to encode the specified 32-bit
        /// unsigned integer with unsigned LEB128 encoding.
        /// </summary>
        private static int GetLengthVariable(uint value)
        {
            if ((value & 0xffffff80) == 0) { return 1; }
            if ((value & 0xffffc000) == 0) { return 2; }
            if ((value & 0xffe00000) == 0) { return 3; }
            if ((value & 0xf0000000) == 0) { return 4; }
            return 5;
        }

        /// <summary>
        /// Gets the number of bytes required to encode the specified 64-bit
        /// unsigned integer with unsigned LEB128 encoding.
        /// </summary>
        private static int GetLengthVariable(ulong value)
        {
            if ((value & 0xffffffffffffff80L) == 0) { return 1; }
            if ((value & 0xffffffffffffc000L) == 0) { return 2; }
            if ((value & 0xffffffffffe00000L) == 0) { return 3; }
            if ((value & 0xfffffffff0000000L) == 0) { return 4; }
            if ((value & 0xfffffff800000000L) == 0) { return 5; }
            if ((value & 0xfffffc0000000000L) == 0) { return 6; }
            if ((value & 0xfffe000000000000L) == 0) { return 7; }
            if ((value & 0xff00000000000000L) == 0) { return 8; }
            if ((value & 0x8000000000000000L) == 0) { return 9; }
            return 10;
        }
    }
}
