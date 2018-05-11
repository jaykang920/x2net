// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2net
{
    // Extensions.ToString
    public static partial class Extensions
    {
        public static string ToStringEx(this bool self)
        {
            return self.ToString();
        }

        public static string ToStringEx(this byte self)
        {
            return self.ToString();
        }

        public static string ToStringEx(this sbyte self)
        {
            return self.ToString();
        }

        public static string ToStringEx(this short self)
        {
            return self.ToString();
        }

        public static string ToStringEx(this int self)
        {
            return self.ToString();
        }

        public static string ToStringEx(this long self)
        {
            return self.ToString();
        }

        public static string ToStringEx(this float self)
        {
            return self.ToString();
        }

        public static string ToStringEx(this double self)
        {
            return self.ToString();
        }

        public static string ToStringEx(this string self)
        {
            if (ReferenceEquals(self, null)) { return "null"; }
            if (self.Length == 0) { return "''"; }
            return String.Format("'{0}'", self.Replace("'", "''"));
        }

        public static string ToStringEx(this DateTime self)
        {
            return String.Format("'{0}'", self);
        }

        public static string ToStringEx(this byte[] self)
        {
            if (ReferenceEquals(self, null)) { return "null"; }
            return BitConverter.ToString(self);
        }

        public static string ToStringEx<T>(this T self) where T : class
        {
            if (ReferenceEquals(self, null)) { return "null"; }
            return self.ToString();
        }

        public static string ToStringEx<T>(this List<T> self)
        {
            if (ReferenceEquals(self, null)) { return "null"; }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append("[");
            bool isValueType = typeof(T).IsValueType;
            for (int i = 0, count = self.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                stringBuilder.Append(' ');
                T entry = self[i];
                if (!isValueType && (object)entry == null)
                {
                    stringBuilder.Append("null");
                }
                else
                {
#if NET40
                    stringBuilder.Append(ToStringEx((dynamic)entry));
#else
                    stringBuilder.Append(entry.ToString());
#endif
                }
            }
            stringBuilder.Append(" ]");

            return stringBuilder.ToString();
        }

        public static string ToStringEx<T, U>(this Dictionary<T, U> self)
        {
            if (ReferenceEquals(self, null)) { return "null"; }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append("{");
            bool first = true;
            bool isKeyValueType = typeof(T).IsValueType;
            bool isValueValueType = typeof(U).IsValueType;
            foreach (var pair in self)
            {
                if (!first) { stringBuilder.Append(','); }
                else { first = false; }

                stringBuilder.Append(' ');
                
                T key = pair.Key;
                if (!isKeyValueType && (object)key == null)
                {
                    stringBuilder.Append("null");
                }
                else
                {
#if NET40
                    stringBuilder.Append(ToStringEx((dynamic)key));
#else
                    stringBuilder.Append(key.ToString());
#endif
                }

                stringBuilder.Append(':');

                U value = pair.Value;
                if (!isValueValueType && (object)value == null)
                {
                    stringBuilder.Append("null");
                }
                else
                {
#if NET40
                    stringBuilder.Append(ToStringEx((dynamic)value));
#else
                    stringBuilder.Append(value.ToString());
#endif
                }
            }
            stringBuilder.Append(" }");

            return stringBuilder.ToString();
        }

#if NET40
        private static string ToStringEx(dynamic value)
        {
            return value.ToStringEx();
        }
#endif
    }
}
