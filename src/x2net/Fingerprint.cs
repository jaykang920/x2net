// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// Manages a fixed-length compact array of bit values.
    /// index ranges over from 0 to length - 1.
    /// </summary>
    public class Fingerprint : IComparable<Fingerprint>, IIndexable<bool>
    {
        private int block;     // primary(default) bit block
        private int[] blocks;  // additional bit blocks
        private readonly int length;

        /// <summary>
        /// Gets the number of bits contained in this Fingerprint.
        /// </summary>
        public int Length
        {
            get { return length; }
        }

        /// <summary>
        /// Gets the minimum number of bytes required to hold all the bits in
        /// this Fingerprint.
        /// </summary>
        private int LengthInBytes
        {
            get { return ((length - 1) >> 3) + 1; }
        }

        /// <summary>
        /// Initializes a new instance of the Fingerprint class that can hold
        /// the specified number of bit values, which are initially set to
        /// <b>false</b>.
        /// </summary>
        public Fingerprint(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.length = length;

            if (length > 32)
            {
                length -= 32;
                blocks = new int[((length - 1) >> 5) + 1];
            }
        }

        /// <summary>
        /// Initializes a new instance of the Fingerprint class that contains
        /// bit values copied from the specified Fingerprint.
        /// </summary>
        public Fingerprint(Fingerprint other)
        {
            block = other.block;
            if (other.blocks != null)
            {
                blocks = (int[])other.blocks.Clone();
            }
            length = other.length;
        }

        /// <summary>
        /// Clears all the bits in the Fingerprint, setting them as <b>false</b>.
        /// </summary>
        public void Clear()
        {
            block = 0;
            if (blocks != null)
            {
                Array.Clear(blocks, 0, blocks.Length);
            }
        }

        /// <summary>
        /// Compares this Fingerprint with the specified Fingerprint object.
        /// </summary>
        /// Implements IComparable(T).CompareTo interface.
        public int CompareTo(Fingerprint other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }
            if (length < other.length)
            {
                return -1;
            }
            else if (length > other.length)
            {
                return 1;
            }
            if (blocks != null)
            {
                for (int i = (blocks.Length - 1); i >= 0; --i)
                {
                    uint thisBlock = (uint)blocks[i];
                    uint otherBlock = (uint)other.blocks[i];
                    if (thisBlock < otherBlock)
                    {
                        return -1;
                    }
                    else if (thisBlock > otherBlock)
                    {
                        return 1;
                    }
                }
            }
            if ((uint)block < (uint)other.block)
            {
                return -1;
            }
            else if ((uint)block > (uint)other.block)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as Fingerprint;
            if ((object)other == null || length != other.length)
            {
                return false;
            }
            if (block != other.block)
            {
                return false;
            }
            if (blocks != null)
            {
                for (int i = 0; i < blocks.Length; ++i)
                {
                    if (blocks[i] != other.blocks[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the hash code for the current object.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = new Hash(Hash.Seed);
            hash.Update(length);
            hash.Update(block);
            if (blocks != null)
            {
                for (int i = 0; i < blocks.Length; ++i)
                {
                    hash.Update(blocks[i]);
                }
            }
            return hash.Code;
        }

        /// <summary>
        /// Determines whether the specified Fingerprint object is equivalent to 
        /// this one.
        /// </summary>
        /// A Fingerprint is said to be equivalent to the other when it covers
        /// all the bits set in the other.
        /// <remarks>
        /// Given two Fingerprint objects x and y, x.Equivalent(y) returns
        /// <b>true</b> if:
        ///   <list type="bullet">
        ///     <item>y.Length is greater than or equal to x.Length</item>
        ///     <item>All the bits set in x are also set in y</item>
        ///   </list>
        /// </remarks>
        public bool Equivalent(Fingerprint other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (length > other.length)
            {
                return false;
            }
            if ((block & other.block) != block)
            {
                return false;
            }
            if (blocks != null)
            {
                for (int i = 0; i < blocks.Length; ++i)
                {
                    int thisBlock = blocks[i];
                    if ((thisBlock & other.blocks[i]) != thisBlock)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Deserialize(Deserializer deserializer)
        {
            int length;
            deserializer.ReadNonnegative(out length);
            int lengthInBytes = ((length - 1) >> 3) + 1;
            int lengthInBlocks = ((lengthInBytes - 1) >> 2) + 1;
            int effectiveBytes = LengthInBytes;

            int count = 0;
            block = 0;
            for (int i = 0; (i < 4) && (count < lengthInBytes); ++i, ++count)
            {
                byte b;
                deserializer.Read(out b);
                if (count < effectiveBytes)
                {
                    block |= ((int)b << (i << 3));
                }
            }
            for (int i = 0; i < lengthInBlocks; ++i)
            {
                int word = 0;
                for (int j = 0; (j < 4) && (count < lengthInBytes); ++j, ++count)
                {
                    byte b;
                    deserializer.Read(out b);
                    if (count < effectiveBytes)
                    {
                        word |= ((int)b << (j << 3));
                    }
                }
                if (blocks != null && i < blocks.Length)
                {
                    blocks[i] = word;
                }
            }
        }

        public int GetLength()
        {
            return Serializer.GetLengthNonnegative(length)
                + LengthInBytes;
        }

        public void Serialize(Serializer serializer)
        {
            serializer.WriteNonnegative(length);
            int lengthInBytes = LengthInBytes;

            int count = 0;
            for (int i = 0; (i < 4) && (count < lengthInBytes); ++i, ++count)
            {
                serializer.Write((byte)(block >> (i << 3)));
            }
            if (blocks == null)
            {
                return;
            }
            for (int i = 0; i < blocks.Length; ++i)
            {
                for (int j = 0; (j < 4) && (count < lengthInBytes); ++j, ++count)
                {
                    serializer.Write((byte)(blocks[i] >> (j << 3)));
                }
            }
        }

        #region Accessors/indexer

        /// <summary>
        /// Gets the bit value at the specified index.
        /// </summary>
        public bool Get(int index)
        {
            if (index < 0 || length <= index)
            {
                throw new IndexOutOfRangeException();
            }
            if ((index & (-1 << 5)) != 0)  // index >= 32
            {
                index -= 32;
                return ((blocks[index >> 5] & (1 << index)) != 0);
            }
            return ((block & (1 << index)) != 0);
        }

        /// <summary>
        /// Sets the bit at the specified index.
        /// </summary>
        public void Touch(int index)
        {
            if (index < 0 || length <= index)
            {
                throw new IndexOutOfRangeException();
            }
            if ((index & (-1 << 5)) != 0)  // index >= 32
            {
                index -= 32;
                blocks[index >> 5] |= (1 << index);
            }
            block |= (1 << index);
        }

        /// <summary>
        /// Clears the bit at the specified index.
        /// </summary>
        public void Wipe(int index)
        {
            if (index < 0 || length <= index)
            {
                throw new IndexOutOfRangeException();
            }
            if ((index & (-1 << 5)) != 0)  // index >= 32
            {
                index -= 32;
                blocks[index >> 5] &= ~(1 << index);
            }
            block &= ~(1 << index);
        }

        /// <summary>
        /// Gets the bit value at the specified index.
        /// </summary>
        public bool this[int index]
        {
            get { return Get(index); }
        }

        #endregion
    }
}
