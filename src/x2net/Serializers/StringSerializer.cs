// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2net
{
    /// <summary>
    /// Default text string serializer.
    /// </summary>
    public class StringSerializer : VerboseSerializer
    {
        private StringBuilder stringBuilder;

        public StringBuilder StringBuilder { get { return stringBuilder; } }

        public StringSerializer()
        {
            stringBuilder = new StringBuilder();
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        // Name-value pair writers for primitive types
        public void Write(string name, bool value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, byte value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, sbyte value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, short value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, int value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, long value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, float value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, double value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, string value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, DateTime value)
        {
            WriteName(name);
            Write(value);
        }

        private void WriteName(string name)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(name);
            stringBuilder.Append('=');
        }

        // Name-value pair writers for composite types
        public void Write(string name, byte[] value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<bool> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<byte> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<sbyte> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<short> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<int> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<long> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<float> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<double> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<string> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<DateTime> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<List<int>> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write(string name, List<List<string>> value)
        {
            WriteName(name);
            Write(value);
        }
        public void Write<T>(string name, List<T> value) where T : Cell
        {
            WriteName(name);
            Write(value);
        }
        public void Write<T>(string name, T value) where T : Cell
        {
            WriteName(name);
            Write(value);
        }

        // Value writers for primitive types
        public void Write(bool value)
        {
            stringBuilder.Append(value);
        }
        public void Write(byte value)
        {
            stringBuilder.Append(value);
        }
        public void Write(sbyte value)
        {
            stringBuilder.Append(value);
        }
        public void Write(short value)
        {
            stringBuilder.Append(value);
        }
        public void Write(int value)
        {
            stringBuilder.Append(value);
        }
        public void Write(long value)
        {
            stringBuilder.Append(value);
        }
        public void Write(float value)
        {
            stringBuilder.Append(value);
        }
        public void Write(double value)
        {
            stringBuilder.Append(value);
        }
        public void Write(string value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('"');
            stringBuilder.Append(value.Replace("\"", "\\\""));
            stringBuilder.Append('"');
        }
        public void Write(DateTime value)
        {
            stringBuilder.Append(value.ToString());
        }

        // Value writers for composite types
        public void Write(byte[] value)
        {

        }
        public void Write(List<bool> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<byte> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<sbyte> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<short> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<int> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<long> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<float> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<double> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<string> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<DateTime> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<List<int>> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write(List<List<string>> value)
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                Write(value[i]);
            }
            stringBuilder.Append(']');
        }
        public void Write<T>(List<T> value) where T : Cell
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append('[');
            for (int i = 0, count = value.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                stringBuilder.Append(' ');
                Write(value[i]);
            }
            stringBuilder.Append(" ]");
        }
        public void Write<T>(T value) where T : Cell
        {
            if (Object.ReferenceEquals(value, null))
            {
                stringBuilder.Append("null");
                return;
            }
            stringBuilder.Append(value.GetTypeTag().RuntimeType.Name);
            stringBuilder.Append(" {");
            Type type = typeof(T);
            bool flag = true;
            if (type != value.GetType())
            {
                value.Serialize(this, type, ref flag);
            }
            else
            {
                value.Serialize(this);
            }
            stringBuilder.Append(" }");
        }
    }
}
