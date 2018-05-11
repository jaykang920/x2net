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

        public static string ToStringEx<T>(this List<T> self)
        {
            if (ReferenceEquals(self, null)) { return "null"; }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append("[");
            for (int i = 0, count = self.Count; i < count; ++i)
            {
                if (i != 0) { stringBuilder.Append(','); }
                stringBuilder.Append(' ');
                T element = self[i];
                if (!typeof(T).IsValueType && (object)element == null)
                {
                    stringBuilder.Append("null");
                }
                else
                {
                    stringBuilder.Append(element.ToString());
                }
            }
            stringBuilder.Append(" ]");

            return stringBuilder.ToString();
        }
    }
}
