// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// Defines methods to write name-value pairs into the backing object.
    /// </summary>
    public interface VerboseSerializer
    {
        // Name-value pair writers for primitive types
        void Write(string name, bool value);
        void Write(string name, byte value);
        void Write(string name, sbyte value);
        void Write(string name, short value);
        void Write(string name, int value);
        void Write(string name, long value);
        void Write(string name, float value);
        void Write(string name, double value);
        void Write(string name, string value);
        void Write(string name, DateTime value);

        // Name-value pair writers for composite types
        void Write(string name, byte[] value);
        void Write(string name, List<bool> value);
        void Write(string name, List<byte> value);
        void Write(string name, List<sbyte> value);
        void Write(string name, List<short> value);
        void Write(string name, List<int> value);
        void Write(string name, List<long> value);
        void Write(string name, List<float> value);
        void Write(string name, List<double> value);
        void Write(string name, List<string> value);
        void Write(string name, List<DateTime> value);
        void Write(string name, List<List<int>> value);
        void Write(string name, List<List<string>> value);
        void Write<T>(string name, List<T> value) where T : Cell;
        void Write<T>(string name, T value) where T : Cell;

        // Value writers for primitive types
        void Write(bool value);
        void Write(byte value);
        void Write(sbyte value);
        void Write(short value);
        void Write(int value);
        void Write(long value);
        void Write(float value);
        void Write(double value);
        void Write(string value);
        void Write(DateTime value);

        // Value writers for composite types
        void Write(byte[] value);
        void Write(List<bool> value);
        void Write(List<byte> value);
        void Write(List<sbyte> value);
        void Write(List<short> value);
        void Write(List<int> value);
        void Write(List<long> value);
        void Write(List<float> value);
        void Write(List<double> value);
        void Write(List<string> value);
        void Write(List<DateTime> value);
        void Write(List<List<int>> value);
        void Write(List<List<string>> value);
        void Write<T>(List<T> value) where T : Cell;
        void Write<T>(T value) where T : Cell;
    }
}
