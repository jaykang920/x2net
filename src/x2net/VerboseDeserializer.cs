// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// Defines methods to read name-value pairs out of the backing object.
    /// </summary>
    public interface VerboseDeserializer
    {
        // Name-value pair readers for primitive types
        void Read(string name, out bool value);
        void Read(string name, out byte value);
        void Read(string name, out sbyte value);
        void Read(string name, out short value);
        void Read(string name, out int value);
        void Read(string name, out long value);
        void Read(string name, out float value);
        void Read(string name, out double value);
        void Read(string name, out string value);
        void Read(string name, out DateTime value);

        // Name-value pair readers for composite types
        void Read(string name, out byte[] value);
        void Read(string name, out List<bool> value);
        void Read(string name, out List<byte> value);
        void Read(string name, out List<sbyte> value);
        void Read(string name, out List<short> value);
        void Read(string name, out List<int> value);
        void Read(string name, out List<long> value);
        void Read(string name, out List<float> value);
        void Read(string name, out List<double> value);
        void Read(string name, out List<string> value);
        void Read(string name, out List<DateTime> value);
        void Read(string name, out List<List<int>> value);
        void Read(string name, out List<List<string>> value);
        void Read<T>(string name, out List<T> value) where T : Cell, new();
        void Read<T>(string name, out T value) where T : Cell, new();

        // Value readers for primitive types
        void Read(out bool value);
        void Read(out byte value);
        void Read(out sbyte value);
        void Read(out short value);
        void Read(out int value);
        void Read(out long value);
        void Read(out float value);
        void Read(out double value);
        void Read(out string value);
        void Read(out DateTime value);

        // Value readers for composite types
        void Read(out byte[] value);
        void Read(out List<bool> value);
        void Read(out List<byte> value);
        void Read(out List<sbyte> value);
        void Read(out List<short> value);
        void Read(out List<int> value);
        void Read(out List<long> value);
        void Read(out List<float> value);
        void Read(out List<double> value);
        void Read(out List<string> value);
        void Read(out List<DateTime> value);
        void Read(out List<List<int>> value);
        void Read(out List<List<string>> value);
        void Read<T>(out List<T> value) where T : Cell, new();
        void Read<T>(out T value) where T : Cell, new();
    }
}
