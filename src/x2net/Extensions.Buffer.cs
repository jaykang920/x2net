// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2net
{
    // Extensions.Buffer
    public static partial class Extensions
    {
        public static int ReadVariable(this Buffer self, out uint value)
        {
            return Deserializer.ReadVariableInternal(self, out value);
        }
    }
}
