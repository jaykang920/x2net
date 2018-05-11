// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2net
{
    /// <summary>
    /// Extension method holder class.
    /// </summary>
    public static partial class Extensions
    {
        public static int ReadVariable(this Buffer self, out uint value)
        {
            return Deserializer.ReadVariableInternal(self, out value);
        }

        public static void Resize<T>(this IList<T> self, int size)
        {
            while (self.Count < size)
            {
                self.Add(default(T));
            }
        }
    }
}
