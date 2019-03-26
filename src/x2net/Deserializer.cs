﻿// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2net
{
    /// <summary>
    /// Binary wire format deserializer.
    /// </summary>
    public sealed class Deserializer
    {
        private Buffer buffer;
        private EventFactory eventFactory;

        /// <summary>
        /// Initializes a new Deserializer object that works on the specified
        /// buffer.
        /// </summary>
        public Deserializer(Buffer buffer)
        {
            this.buffer = buffer;
        }

        /// <summary>
        /// Initializes a new Deserializer object that works on the specified
        /// buffer and event factory.
        /// </summary>
        public Deserializer(Buffer buffer, EventFactory eventFactory)
        {
            this.buffer = buffer;
            this.eventFactory = eventFactory;
        }

        // Overloaded Read for primitive types

        /// <summary>
        /// Decodes a boolean value out of the underlying buffer.
        /// </summary>
        public void Read(out bool value)
        {
            buffer.CheckLengthToRead(1);
            value = (buffer.GetByte() != 0);
        }

        /// <summary>
        /// Decodes a single byte out of the underlying buffer.
        /// </summary>
        public void Read(out byte value)
        {
            buffer.CheckLengthToRead(1);
            value = buffer.GetByte();
        }

        /// <summary>
        /// Decodes an 8-bit signed integer out of the underlying buffer.
        /// </summary>
        public void Read(out sbyte value)
        {
            buffer.CheckLengthToRead(1);
            value = (sbyte)buffer.GetByte();
        }

        /// <summary>
        /// Decodes a 16-bit signed integer out of the underlying buffer.
        /// </summary>
        public void Read(out short value)
        {
            buffer.CheckLengthToRead(2);
            value = (short)buffer.GetByte();
            value = (short)((value << 8) | buffer.GetByte());
        }

        /// <summary>
        /// Decodes a 32-bit signed integer out of the underlying buffer.
        /// </summary>
        public int Read(out int value)
        {
            // Zigzag decoding
            uint u;
            int bytes = ReadVariable(out u);
            value = (int)(u >> 1) ^ -((int)u & 1);
            return bytes;
        }

        /// <summary>
        /// Decodes a 32-bit non-negative integer out of the underlying buffer.
        /// </summary>
        public int ReadNonnegative(out int value)
        {
            uint unsigned;
            int bytes = ReadVariable(out unsigned);
            if (unsigned > Int32.MaxValue) { throw new OverflowException(); }
            value = (int)unsigned;
            return bytes;
        }

        /// <summary>
        /// Decodes a 64-bit signed integer out of the underlying buffer.
        /// </summary>
        public int Read(out long value)
        {
            // Zigzag decoding
            ulong u;
            int bytes = ReadVariable(out u);
            value = (long)(u >> 1) ^ -((long)u & 1);
            return bytes;
        }

        /// <summary>
        /// Decodes a 32-bit floating-point number out of the underlying buffer.
        /// </summary>
        public void Read(out float value)
        {
            int i;
            ReadFixed(out i);
            value = BitConverter.ToSingle(System.BitConverter.GetBytes(i), 0);
        }

        /// <summary>
        /// Decodes a 64-bit floating-point number out of the underlying buffer.
        /// </summary>
        public void Read(out double value)
        {
            long l;
            ReadFixed(out l);
            value = BitConverter.ToDouble(System.BitConverter.GetBytes(l), 0);
        }

        /// <summary>
        /// Decodes a text string out of the underlying buffer.
        /// </summary>
        public void Read(out string value)
        {
            // UTF-8 decoding
            int length;
            ReadNonnegative(out length);
            if (length == 0)
            {
                value = String.Empty;
                return;
            }
            buffer.CheckLengthToRead(length);
            char c, c2, c3;
            int bytesRead = 0;
            var stringBuilder = new StringBuilder(length);
            while (bytesRead < length)
            {
                c = (char)buffer.GetByte();
                switch (c >> 4)
                {
                    case 0: case 1: case 2: case 3: case 4: case 5: case 6: case 7:
                        // 0xxxxxxx
                        ++bytesRead;
                        stringBuilder.Append(c);
                        break;
                    case 12: case 13:
                        // 110x xxxx  10xx xxxx
                        bytesRead += 2;
                        if (bytesRead > length)
                        {
                            throw new InvalidEncodingException();
                        }
                        c2 = (char)buffer.GetByte();
                        if ((c2 & 0xc0) != 0x80)
                        {
                            throw new InvalidEncodingException();
                        }
                        stringBuilder.Append((char)(((c & 0x1f) << 6) | (c2 & 0x3f)));
                        break;
                    case 14:
                        // 1110 xxxx  10xx xxxx  10xx xxxx
                        bytesRead += 3;
                        if (bytesRead > length)
                        {
                            throw new InvalidEncodingException();
                        }
                        c2 = (char)buffer.GetByte();
                        c3 = (char)buffer.GetByte();
                        if (((c2 & 0xc0) != 0x80) || ((c3 & 0xc0) != 0x80))
                        {
                            throw new InvalidEncodingException();
                        }
                        stringBuilder.Append((char)(((c & 0x0f) << 12) |
                          ((c2 & 0x3f) << 6) | ((c3 & 0x3f) << 0)));
                        break;
                    default:
                        // 10xx xxxx  1111 xxxx
                        throw new InvalidEncodingException();
                }
            }
            value = stringBuilder.ToString();
        }

        /// <summary>
        /// Decodes a datetime value out of the underlying buffer.
        /// </summary>
        public void Read(out DateTime value)
        {
            long milliseconds;
            ReadFixed(out milliseconds);
            DateTime unixEpoch = new DateTime(621355968000000000);
            value = unixEpoch.AddTicks(milliseconds * TimeSpan.TicksPerMillisecond);
        }

        // Overloaded Read for composite types

        /// <summary>
        /// Decodes an array of bytes out of the underlying buffer.
        /// </summary>
        public void Read(out byte[] value)
        {
            int length;
            ReadNonnegative(out length);
            buffer.CheckLengthToRead(length);
            value = new byte[length];
            buffer.Read(value, 0, length);
        }

#if !NET40
        // Obsolete list support without dynamic keyword

        /// <summary>
        /// Decodes an ordered list of boolean values out of the underlying buffer.
        /// </summary>
        public void Read(out List<bool> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<bool>();
            for (int i = 0; i < count; ++i)
            {
                bool element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of byte values out of the underlying buffer.
        /// </summary>
        public void Read(out List<byte> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<byte>();
            for (int i = 0; i < count; ++i)
            {
                byte element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of 8-bit signed integers out of the underlying buffer.
        /// </summary>
        public void Read(out List<sbyte> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<sbyte>();
            for (int i = 0; i < count; ++i)
            {
                sbyte element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of 16-bit signed integers out of the underlying buffer.
        /// </summary>
        public void Read(out List<short> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<short>();
            for (int i = 0; i < count; ++i)
            {
                short element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of 32-bit signed integers out of the underlying buffer.
        /// </summary>
        public void Read(out List<int> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<int>();
            for (int i = 0; i < count; ++i)
            {
                int element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of 64-bit signed integers out of the underlying buffer.
        /// </summary>
        public void Read(out List<long> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<long>();
            for (int i = 0; i < count; ++i)
            {
                long element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of 32-bit floating-point values out of the
        /// underlying buffer.
        /// </summary>
        public void Read(out List<float> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<float>();
            for (int i = 0; i < count; ++i)
            {
                float element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of 64-bit floating-point values out of the
        /// underlying buffer.
        /// </summary>
        public void Read(out List<double> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<double>();
            for (int i = 0; i < count; ++i)
            {
                double element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of text strings out of the underlying buffer.
        /// </summary>
        public void Read(out List<string> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<string>();
            for (int i = 0; i < count; ++i)
            {
                string element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of DateTime values out of the underlying buffer.
        /// </summary>
        public void Read(out List<DateTime> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<DateTime>();
            for (int i = 0; i < count; ++i)
            {
                DateTime element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of Int32 lists out of the underlying buffer.
        /// </summary>
        public void Read(out List<List<int>> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<List<int>>();
            for (int i = 0; i < count; ++i)
            {
                List<int> element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of string lists out of the underlying buffer.
        /// </summary>
        public void Read(out List<List<string>> value)
        {
            int count;
            ReadNonnegative(out count);
            value = new List<List<string>>();
            for (int i = 0; i < count; ++i)
            {
                List<string> element;
                Read(out element);
                value.Add(element);
            }
        }

        /// <summary>
        /// Decodes an ordered list of Cell-derived objects out of the
        /// underlying buffer.
        /// </summary>
        public void Read<T>(out List<T> value) where T : Cell, new()
        {
            int count;
            ReadNonnegative(out count);
            value = new List<T>();
            for (int i = 0; i < count; ++i)
            {
                T element;
                Read(out element);
                value.Add(element);
            }
        }
#endif

        /// <summary>
        /// Decodes a Cell-derived objects out of the underlying buffer.
        /// </summary>
        public void Read<T>(out T value) where T : Cell, new()
        {
            value = null;
            int length;
            ReadNonnegative(out length);
            if (length == 0) { return; }

            int marker = buffer.Position + length;

            Type type = typeof(T);
            Type eventType = typeof(Event);
            if (type.IsSubclassOf(eventType) || type == eventType)
            {
                int typeId;
                Read(out typeId);

                Event e = null;
                if (!ReferenceEquals(eventFactory, null))
                {
                    e = eventFactory.Create(typeId);
                }
                if (ReferenceEquals(e, null))
                {
                    e = EventFactory.Global.Create(typeId);
                }
                if (ReferenceEquals(e, null))
                {
                    Trace.Info("error deserializing {0}: unknown event type id {1}",
                        type.Name, typeId);
                }

                value = (T)((Cell)e);
            }
            else
            {
                value = new T();
            }

            if (!ReferenceEquals(value, null))
            {
                value.Deserialize(this);
            }

            if (buffer.Position != marker)
            {
                buffer.Position = marker;
            }
        }

#if NET40
        // Generic list/map support with C# 4.0 dynamic keyword

        public void Read<T>(out List<T> list)
        {
            int count;
            ReadNonnegative(out count);
            list = new List<T>();
            for (int i = 0; i < count; ++i)
            {
                T entry = Default<T>();
                entry = (T)Read_((dynamic)entry);
                list.Add(entry);
            }
        }

        public void Read<T, U>(out Dictionary<T, U> map)
        {
            int count;
            ReadNonnegative(out count);
            map = new Dictionary<T, U>();
            for (int i = 0; i < count; ++i)
            {
                T key = Default<T>();
                U value = Default<U>();
                key = (T)Read_((dynamic)key);
                value = (U)Read_((dynamic)value);
                map.Add(key, value);
            }
        }

        private T Default<T>()
        {
            var type = typeof(T);
            if (type.IsValueType)
            {
                return default(T);
            }
            if (type == typeof(string))
            {
                return (T)(object)String.Empty;
            }
            if (type == typeof(byte[]))
            {
                return (T)(object)new byte[0];
            }
            return Activator.CreateInstance<T>();
        }

        private dynamic Read_(dynamic value)
        {
            return Read_(value);
        }

        private bool Read_(bool value)
        {
            Read(out value);
            return value;
        }
        private byte Read_(byte value)
        {
            Read(out value);
            return value;
        }
        private sbyte Read_(sbyte value)
        {
            Read(out value);
            return value;
        }
        private short Read_(short value)
        {
            Read(out value);
            return value;
        }
        private int Read_(int value)
        {
            Read(out value);
            return value;
        }
        private long Read_(long value)
        {
            Read(out value);
            return value;
        }
        private float Read_(float value)
        {
            Read(out value);
            return value;
        }
        private double Read_(double value)
        {
            Read(out value);
            return value;
        }
        private string Read_(string value)
        {
            Read(out value);
            return value;
        }
        private DateTime Read_(DateTime value)
        {
            Read(out value);
            return value;
        }
        private byte[] Read_(byte[] value)
        {
            Read(out value);
            return value;
        }
        public T Read_<T>(T value) where T : Cell, new()
        {
            Read(out value);
            return value;
        }
        public List<T> Read_<T>(List<T> value)
        {
            Read(out value);
            return value;
        }
        public Dictionary<T, U> Read_<T, U>(Dictionary<T, U> value)
        {
            Read(out value);
            return value;
        }
#endif

        // Read helper methods

        /// <summary>
        /// Decodes a 32-bit signed integer by fixed-width big-endian byte order.
        /// </summary>
        private void ReadFixed(out int value)
        {
            buffer.CheckLengthToRead(4);
            value = buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
        }

        /// <summary>
        /// Decodes a 64-bit signed integer by fixed-width big-endian byte order.
        /// </summary>
        private void ReadFixed(out long value)
        {
            buffer.CheckLengthToRead(8);
            value = buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
            value = (value << 8) | buffer.GetByte();
        }

        /// <summary>
        /// Decodes a 32-bit unsigned integer out of the underlying buffer,
        /// with unsigned LEB128 decoding.
        /// </summary>
        public int ReadVariable(out uint value)
        {
            return ReadVariableInternal(buffer, out value);
        }

        internal static int ReadVariableInternal(Buffer buffer, out uint value)
        {
            // Unsigned LEB128 decoding
            value = 0U;
            int i, shift = 0;
            for (i = 0; i < 5; ++i)
            {
                buffer.CheckLengthToRead(1);
                byte b = buffer.GetByte();
                value |= (((uint)b & 0x7fU) << shift);
                if ((b & 0x80) == 0)
                {
                    break;
                }
                shift += 7;
            }
            return (i < 5 ? (i + 1) : 5);
        }

        /// <summary>
        /// Decodes a 64-bit unsigned integer out of the underlying buffer,
        /// with unsigned LEB128 decoding.
        /// </summary>
        public int ReadVariable(out ulong value)
        {
            // Unsigned LEB128 decoding
            value = 0UL;
            int i, shift = 0;
            for (i = 0; i < 10; ++i)
            {
                buffer.CheckLengthToRead(1);
                byte b = buffer.GetByte();
                value |= (((ulong)b & 0x7fU) << shift);
                if ((b & 0x80) == 0)
                {
                    break;
                }
                shift += 7;
            }
            return (i < 10 ? (i + 1) : 0);
        }
    }
}
