// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// A trivial example of BufferTransform that just invert every bit.
    /// </summary>
    public class Inverse : IBufferTransform
    {
        public int HandshakeBlockLength { get { return 0; } }

        public object Clone()
        {
            return new Inverse();
        }

        public void Dispose()
        {
        }

        public byte[] InitializeHandshake()
        {
            return null;
        }

        public byte[] Handshake(byte[] challenge)
        {
            return null;
        }

        public bool FinalizeHandshake(byte[] response)
        {
            return true;
        }

        public int Transform(Buffer buffer, int length)
        {
            for (int i = (int)buffer.Length - length; i < buffer.Length; ++i)
            {
                buffer[i] = (byte)~buffer[i];
            }
            return length;
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                buffer[i] = (byte)~buffer[i];
            }
            return length;
        }
    }
}
