// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// Defines methods to be implemented by concrete buffer transforms.
    /// </summary>
    public interface IBufferTransform : IDisposable
    {
        int HandshakeBlockLength { get; }

        object Clone();

        byte[] InitializeHandshake();
        byte[] Handshake(byte[] challenge);
        bool FinalizeHandshake(byte[] response);

        /// <summary>
        /// Transform the specified trailing byte(s) of the buffer.
        /// </summary>
        int Transform(Buffer buffer, int length);
        /// <summary>
        /// Inverse transform the specified leading byte(s) of the buffer.
        /// </summary>
        int InverseTransform(Buffer buffer, int length);
    }

    public sealed class BufferTransformStack : IBufferTransform
    {
        private List<IBufferTransform> transforms;

        public int HandshakeBlockLength
        {
            get
            {
                int result = 0;
                for (int i = 0, count = transforms.Count; i < count; ++i)
                {
                    result += transforms[i].HandshakeBlockLength;
                }
                return result;
            }
        }

        public BufferTransformStack()
        {
            transforms = new List<IBufferTransform>();
        }

        private BufferTransformStack(IList<IBufferTransform> transforms)
        {
            this.transforms = new List<IBufferTransform>();
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                this.transforms.Add((IBufferTransform)transforms[i].Clone());
            }
        }

        public object Clone()
        {
            return new BufferTransformStack(transforms);
        }

        public void Dispose()
        {
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                transforms[i].Dispose();
            }
            transforms.Clear();
        }

        public byte[] InitializeHandshake()
        {
            byte[] result = null;
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                result = result.Concat(transforms[i].InitializeHandshake());
            }
            return result;
        }

        public byte[] Handshake(byte[] challenge)
        {
            byte[] result = null;
            int offset = 0;
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                var transform = transforms[i];
                var blockLength = transform.HandshakeBlockLength;
                if (blockLength > 0)
                {
                    var block = challenge.SubArray(offset, blockLength);
                    result = result.Concat(transforms[i].Handshake(block));
                    offset += blockLength;
                }
            }
            return result;
        }

        public bool FinalizeHandshake(byte[] response)
        {
            if (response == null) 
            { 
                return false; 
            }
            int offset = 0;
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                var transform = transforms[i];
                var blockLength = transform.HandshakeBlockLength;
                if (blockLength > 0)
                {
                    var block = response.SubArray(offset, blockLength);
                    if (!transforms[i].FinalizeHandshake(block))
                    {
                        return false;
                    }
                    offset += blockLength;
                }
            }
            return true;
        }

        public BufferTransformStack Add(IBufferTransform transform)
        {
            if (!transforms.Contains(transform))
            {
                transforms.Add(transform);
            }
            return this;
        }

        public BufferTransformStack Remove(IBufferTransform transform)
        {
            transforms.Remove(transform);
            return this;
        }

        public int Transform(Buffer buffer, int length)
        {
            var count = transforms.Count;
            for (var i = 0; i < count; ++i)
            {
                length = transforms[i].Transform(buffer, length);
            }
            return length;
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            var count = transforms.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                length = transforms[i].InverseTransform(buffer, length);
            }
            return length;
        }
    }
}
