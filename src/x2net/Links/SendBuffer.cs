// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    public class SendBuffer : IDisposable
    {
        private byte[] headerBytes;
        private int headerLength;
        private Buffer buffer;

        public byte[] HeaderBytes { get { return headerBytes; } }
        public int HeaderLength
        {
            get { return headerLength; }
            set { headerLength = value; }
        }
        public Buffer Buffer { get { return buffer; } }
        public int Length { get { return (headerLength + (int)buffer.Length); } }

        public SendBuffer()
        {
            headerBytes = new byte[5];
            buffer = new Buffer();
        }

        ~SendBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                buffer.Dispose();
            }
            catch (Exception) { }
        }

        public void ListOccupiedSegments(IList<ArraySegment<byte>> blockList)
        {
            blockList.Add(new ArraySegment<byte>(headerBytes, 0, headerLength));
            buffer.ListOccupiedSegments(blockList);
        }

        public void Reset()
        {
            headerLength = 0;
            buffer.Reset();
        }
    }

}
