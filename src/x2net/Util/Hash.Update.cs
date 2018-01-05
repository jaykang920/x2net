// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    // Hash.Update
    public partial struct Hash
    {
        public void Update(bool value)
        {
            Code = Update(Code, value);
        }

        public void Update(sbyte value)
        {
            Code = Update(Code, value);
        }

        public void Update(byte value)
        {
            Code = Update(Code, value);
        }

        public void Update(short value)
        {
            Code = Update(Code, value);
        }

        public void Update(ushort value)
        {
            Code = Update(Code, value);
        }

        public void Update(int value)
        {
            Code = Update(Code, value);
        }

        public void Update(uint value)
        {
            Code = Update(Code, value);
        }

        public void Update(long value)
        {
            Code = Update(Code, value);
        }

        public void Update(ulong value)
        {
            Code = Update(Code, value);
        }

        public void Update(float value)
        {
            Code = Update(Code, value);
        }

        public void Update(double value)
        {
            Code = Update(Code, value);
        }

        public void Update(string value)
        {
            Code = Update(Code, value);
        }

        public void Update(DateTime value)
        {
            Code = Update(Code, value);
        }

        public void Update(byte[] value)
        {
            Code = Update(Code, value);
        }

        public void Update<T>(List<T> value)
        {
            Code = Update(Code, value);
        }

        public void Update<T>(T value)
        {
            Code = Update(Code, value);
        }
    }
}
