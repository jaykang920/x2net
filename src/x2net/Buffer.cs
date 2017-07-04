// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// A variable-length byte buffer class whose capacity is limited to a
    /// multiple of a power of 2.
    /// </summary>
    public sealed class Buffer : IDisposable
    {
        static readonly int sizeExponent = Config.Buffer.SizeExponent.Segment;
        static readonly int remainderMask = ~(~0 << sizeExponent);
        static readonly int minLevel = Config.Buffer.RoomFactor.MinLevel;
        static readonly int maxLevel = Config.Buffer.RoomFactor.MaxLevel;

        private List<Segment> blocks;

        private Segment current;
        private int currentIndex;

        private int position;
        private int back;
        private int front;

        private int marker;

        private int level;  // read buffer room factor level

        /// <summary>
        /// Gets the block size in bytes.
        /// </summary>
        public int BlockSize
        {
            get { return (1 << sizeExponent); }
        }

        /// <summary>
        /// Gets the maximum capacity of the buffer.
        /// </summary>
        public int Capacity
        {
            get { return (BlockSize * blocks.Count); }
        }

        /// <summary>
        /// Checks whether the buffer is empty (i.e. whether its length is 0).
        /// </summary>
        public bool IsEmpty
        {
            get { return (front == back); }
        }

        /// <summary>
        /// Gets the length of the buffered bytes.
        /// </summary>
        public long Length
        {
            get { return (long)(back - front); }
        }

        /// <summary>
        /// Gets or sets the current zero-based position.
        /// </summary>
        public int Position
        {
            get
            {
                return (position - front);
            }
            set
            {
                int adjusted = value + front;
                if (adjusted < front || back < adjusted)
                {
                    throw new IndexOutOfRangeException();
                }
                position = adjusted;
                int blockIndex = position >> sizeExponent;
                if ((blockIndex != 0) && ((position & remainderMask) == 0))
                {
                    --blockIndex;
                }
                if (blockIndex != currentIndex)
                {
                    currentIndex = blockIndex;
                    current = blocks[currentIndex];
                }
            }
        }

        /// <summary>
        /// Gets the first segment of this buffer.
        /// </summary>
        public Segment FirstSegment
        {
            get
            {
                if (blocks.Count < 1)
                {
                    blocks.Add(SegmentPool.Acquire());
                }
                return blocks[0];
            }
        }

        /// <summary>
        /// Initializes a new instance of the Buffer class.
        /// </summary>
        public Buffer()
        {
            if (sizeExponent < 0 || 31 < sizeExponent)
            {
                throw new ArgumentOutOfRangeException();
            }
            blocks = new List<Segment>();

            blocks.Add(SegmentPool.Acquire());

            currentIndex = 0;
            current = blocks[currentIndex];
            position = 0;
            back = 0;
            front = 0;

            marker = -1;
        }

        /// <summary>
        /// Destructor to return blocks to the pool
        /// </summary>
        ~Buffer()
        {
            Cleanup();
        }

        /// <summary>
        /// Implments IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        public void CopyFrom(byte[] buffer, int offset, int length)
        {
            EnsureCapacityToWrite(length);
            int blockIndex = position >> sizeExponent;
            int dstOffset = position & remainderMask;
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < length)
            {
                bytesToCopy = Math.Min(BlockSize - dstOffset, length - bytesCopied);
                Segment block = blocks[blockIndex++];
                System.Buffer.BlockCopy(buffer, offset + bytesCopied,
                  block.Array, block.Offset + dstOffset, bytesToCopy);
                dstOffset = 0;
                bytesCopied += bytesToCopy;
            }
            Position = Position + length;
        }

        private void CopyTo(byte[] buffer, int offset, int length)
        {
            int blockIndex = offset >> sizeExponent;
            int srcOffset = offset & remainderMask;
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < length)
            {
                bytesToCopy = Math.Min(BlockSize - srcOffset, length - bytesCopied);
                Segment block = blocks[blockIndex++];
                System.Buffer.BlockCopy(block.Array, block.Offset + srcOffset,
                  buffer, bytesCopied, bytesToCopy);
                srcOffset = 0;
                bytesCopied += bytesToCopy;
            }
        }

        public void ListOccupiedSegments(IList<ArraySegment<byte>> blockList)
        {
            ListSegments(blockList, front, back);
        }

        public void ListStartingSegments(IList<ArraySegment<byte>> blockList, int length)
        {
            ListSegments(blockList, front, front + length);
        }

        public void ListEndingSegments(IList<ArraySegment<byte>> blockList, int length)
        {
            ListSegments(blockList, back - length, back);
        }

        private void ListSegments(IList<ArraySegment<byte>> blockList, int begin, int end)
        {
            int beginIndex = begin >> sizeExponent;
            int beginOffset = begin & remainderMask;
            int endIndex = end >> sizeExponent;
            int endOffset = end & remainderMask;
            if (beginIndex == endIndex)
            {
                blockList.Add(new ArraySegment<byte>(
                    blocks[beginIndex].Array,
                    blocks[beginIndex].Offset + beginOffset,
                    endOffset - beginOffset));
                return;
            }
            blockList.Add(new ArraySegment<byte>(
                blocks[beginIndex].Array,
                blocks[beginIndex].Offset + beginOffset,
                BlockSize - beginOffset));
            for (int i = beginIndex + 1; i < endIndex; ++i)
            {
                blockList.Add(new ArraySegment<byte>(
                    blocks[i].Array,
                    blocks[i].Offset,
                    BlockSize));
            }
            if (endOffset != 0)
            {
                blockList.Add(new ArraySegment<byte>(
                    blocks[endIndex].Array,
                    blocks[endIndex].Offset,
                    endOffset));
            }
        }

        public void ListAvailableSegments(IList<ArraySegment<byte>> blockList)
        {
            if (back < Capacity)
            {
                if (level > minLevel) { --level; }
            }
            else
            {
                if (level < maxLevel) { ++level; }
            }
            int roomFactor = 1 << level;
            int numWholeBlocks = (Capacity - back) >> sizeExponent;
            if (numWholeBlocks < roomFactor)
            {
                int count = (roomFactor - numWholeBlocks);
                for (int i = 0; i < count; ++i)
                {
                    blocks.Add(SegmentPool.Acquire());
                }
            }

            int backIndex = back >> sizeExponent;
            int backOffset = back & remainderMask;
            blockList.Add(new ArraySegment<byte>(
                blocks[backIndex].Array,
                blocks[backIndex].Offset + backOffset,
                BlockSize - backOffset));
            for (int i = backIndex + 1, count = blocks.Count; i < count; ++i)
            {
                blockList.Add(new ArraySegment<byte>(
                    blocks[i].Array,
                    blocks[i].Offset,
                    BlockSize));
            }
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            CheckLengthToRead(count);
            CopyTo(buffer, position, count);
            Position = Position + count;
        }

        public void Reset()
        {
            Position = 0;
            back = front;
        }

        /// <summary>
        /// Alias of (Position = 0).
        /// </summary>
        public void Rewind()
        {
            Position = 0;
        }

        public void Shrink(int numBytes)
        {
            if ((front + numBytes) > back)
            {
                throw new ArgumentOutOfRangeException();
            }
            front += numBytes;
            if (position < front)
            {
                Position = 0;
            }
        }

        public void Stretch(int numBytes)
        {
            if ((back + numBytes) > Capacity)
            {
                throw new ArgumentOutOfRangeException();
            }
            back += numBytes;
        }

        /// <summary>
        /// Returns a byte array containing all the bytes in this buffer.
        /// </summary>
        public byte[] ToArray()
        {
            byte[] array = new byte[Length];
            CopyTo(array, front, (int)Length);
            return array;
        }

        /// <summary>
        /// Gets or sets the byte at the specified index.
        /// </summary>
        public byte this[int index]
        {
            get
            {
                index += front;
                Segment block = blocks[index >> sizeExponent];
                return block.Array[block.Offset + (index & remainderMask)];
            }
            set
            {
                index += front;
                Segment block = blocks[index >> sizeExponent];
                block.Array[block.Offset + (index & remainderMask)] = value;
            }
        }

        public void Trim()
        {
            int index, count;
            if (marker >= 0)
            {
                if (position < marker)
                {
                    Position = (marker - front);
                }
                marker = -1;
            }

            if (position == back)
            {
                index = 1;
                count = blocks.Count - 1;
                front = back = 0;
            }
            else
            {
                index = 0;
                count = position >> sizeExponent;
                if (count >= blocks.Count)
                {
                    count = blocks.Count - 1;
                }
                back -= BlockSize * count;
                front = position & remainderMask;
            }

            if (count > 0)
            {
                int roomFactor = 1 << level;
                List<Segment> blocksToRemove = blocks.GetRange(index, count);
                blocks.RemoveRange(index, count);
                for (int i = 0; i < blocksToRemove.Count; ++i)
                {
                    Segment block = blocksToRemove[i];
                    if (i < roomFactor)
                    {
                        blocks.Add(block);
                    }
                    else
                    {
                        SegmentPool.Release(blocksToRemove[i]);
                    }
                }
            }
            Position = 0;
        }

        public void MarkToRead(int lengthToRead)
        {
            if ((front + lengthToRead) > back)
            {
                throw new ArgumentOutOfRangeException();
            }
            marker = front + lengthToRead;
        }

        public void Write(byte[] value, int offset, int count)
        {
            //WriteVariable(count);
            CopyFrom(value, offset, count);
        }

        public void CheckLengthToRead(int numBytes)
        {
            int limit = (marker >= 0 ? marker : back);
            if ((position + numBytes) > limit)
            {
                Trace.Warn("front={0} pos={1} back={2} marker={3} numBytes={4}", front, position, back, marker, numBytes);

                throw new System.IO.EndOfStreamException();
            }
        }

        public void EnsureCapacityToWrite(int numBytes)
        {
            int required = position + numBytes;
            while (required >= Capacity)
            {
                blocks.Add(SegmentPool.Acquire());
            }
            if (required > back)
            {
                back = required;
            }
        }

        public byte GetByte()
        {
            BlockFeed();
            return current.Array[current.Offset + (position++ & remainderMask)];
        }

        public void PutByte(byte value)
        {
            BlockFeed();
            current.Array[current.Offset + (position++ & remainderMask)] = value;
        }

        private void BlockFeed()
        {
            if (((position & remainderMask) == 0) &&
                ((position & ~remainderMask) != 0))
            {
                current = blocks[++currentIndex];
            }
        }

        private void Cleanup()
        {
            if (blocks.Count == 0)
            {
                return;
            }
            for (int i = 0, count = blocks.Count; i < count; ++i)
            {
                SegmentPool.Release(blocks[i]);
            }
            blocks.Clear();
            current = new Segment();
        }
    }
}
