// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// A reduced form of the ArraySegment(byte) struct, assuming a known
    /// constant count value.
    /// </summary>
    public struct Segment
    {
        private byte[] array;
        private int offset;

        /// <summary>
        /// Gets the backing byte array containing the range of bytes that this
        /// segment delimits.
        /// </summary>
        public byte[] Array { get { return array; } }

        /// <summary>
        /// Gets the position of the first byte in the range delimited by this
        /// segment, relative to the start of the backing byte array.
        /// </summary>
        public int Offset { get { return offset; } }

        /// <summary>
        /// Initializes a new instance of the Segment structure that delimits
        /// the specified range of the bytes in the specified byte array.
        /// </summary>
        public Segment(byte[] array, int offset)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            this.array = array;
            this.offset = offset;
        }

        public override bool Equals(Object obj)
        {
            if (obj is Segment)
            {
                return Equals((Segment)obj);
            }
            return false;
        }

        public bool Equals(Segment obj)
        {
            return obj.array == array && obj.offset == offset;
        }

        public override int GetHashCode()
        {
            return array.GetHashCode() ^ offset;
        }

        // Operators

        public static bool operator ==(Segment x, Segment y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Segment x, Segment y)
        {
            return !(x == y);
        }
    } 

    /// <summary>
    /// Manages a single large buffer block as if it's a pool of smaller segments.
    /// </summary>
    public sealed class SegmentedBuffer
    {
        private static readonly int chunkSize = Config.Buffer.ChunkSize;
        private static readonly int segmentSize = Config.Buffer.SegmentSize;

        private byte[] buffer;
        private int offset;
        private Stack<int> available;

        private object syncRoot = new Object();

        /// <summary>
        /// Initializes a new instance of the SegmentedBuffer class.
        /// </summary>
        public SegmentedBuffer()
        {
            buffer = new byte[chunkSize];
            available = new Stack<int>();
        }

        /// <summary>
        /// Tries to acquire an available buffer segment.
        /// </summary>
        /// <returns>true if successful, false if there is no available segment.
        /// </returns>
        public bool Acquire(ref Segment segment)
        {
            lock (available)
            {
                if (available.Count > 0)
                {
                    segment = new Segment(buffer, available.Pop());
                    return true;
                }
            }

            int position;
            lock (syncRoot)
            {
                if ((chunkSize - segmentSize) < offset)
                {
                    return false;
                }
                position = offset;
                offset += segmentSize;
            }
            segment = new Segment(buffer, position);
            return true;
        }

        /// <summary>
        /// Tries to return the specified segment back to the pool.
        /// </summary>
        /// <returns>true if successful, false if the specified segment does not
        /// belong to this pool.</returns>
        public bool Release(Segment segment)
        {
            if (segment.Array != buffer)
            {
                return false;
            }
            lock (available)
            {
                available.Push(segment.Offset);
            }
            return true;
        }
    }

    /// <summary>
    /// Manages a pool of fixed-length (2^n) byte array segments.
    /// </summary>
    public static class SegmentPool
    {
        private static List<SegmentedBuffer> pools;

        private static ReaderWriterLockSlim rwlock;

        static SegmentPool()
        {
            pools = new List<SegmentedBuffer>();
            rwlock = new ReaderWriterLockSlim();

            using (new WriteLock(rwlock))
            {
                pools.Add(new SegmentedBuffer());
            }
        }

        /// <summary>
        /// Acquires an avilable segment from the pool.
        /// </summary>
        public static Segment Acquire()
        {
            Segment result = new Segment();
            using (new UpgradeableReadLock(rwlock))
            {
                for (int i = 0, count = pools.Count; i < count; ++i)
                {
                    if (pools[i].Acquire(ref result))
                    {
                        return result;
                    }
                }
                using (new WriteLock(rwlock))
                {
                    for (int i = 0, count = pools.Count; i < count; ++i)
                    {
                        if (pools[i].Acquire(ref result))
                        {
                            return result;
                        }
                    }
                    var pool = new SegmentedBuffer();
                    pools.Add(pool);
                    pool.Acquire(ref result);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the specified segment back to the pool.
        /// </summary>
        public static void Release(Segment segment)
        {
            using (new ReadLock(rwlock))
            {
                for (int i = 0, count = pools.Count; i < count; ++i)
                {
                    if (pools[i].Release(segment))
                    {
                        return;
                    }
                }
            }
        }
    }
}
