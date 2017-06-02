// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2net
{
    /// <summary>
    /// Compact pool of consecutive integer values in a finite range.
    /// </summary>
    public class RangedIntPool
    {
        private bool advancing;
        private int minValue;
        private int maxValue;
        private int offset;
        private BitArray bitArray;

        /// <summary>
        /// Gets the number of consecutive integers handled by this pool.
        /// </summary>
        public int Length { get { return bitArray.Length; } }

        /// <summary>
        /// Initializes a new instance of the RangedIntPool class, containing
        /// integers of range [0, maxValue].
        /// </summary>
        public RangedIntPool(int maxValue)
            : this (0, maxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RangedIntPool class with the
        /// specified circulation behavior, containing integers of range
        /// [0, maxValue].
        /// </summary>
        public RangedIntPool(int maxValue, bool advancing)
            : this(0, maxValue, advancing)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RangedIntPool class, containing
        /// integers of range [minValue, maxValue].
        /// </summary>
        public RangedIntPool(int minValue, int maxValue)
            : this (minValue, maxValue, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RangedIntPool class with the
        /// specified circulation behavior, containing integers of range
        /// [minValue, maxValue].
        /// </summary>
        public RangedIntPool(int minValue, int maxValue, bool advancing)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentException();
            }
            this.advancing = advancing;
            this.minValue = minValue;
            this.maxValue = maxValue;
            bitArray = new BitArray(maxValue - minValue + 1);
        }

        /// <summary>
        /// Gets the next available value from the pool.
        /// </summary>
        public int Acquire()
        {
            lock (bitArray)
            {
                int index = offset;
                for (int i = 0, length = Length; i < length; ++i, ++index)
                {
                    if (index >= length)
                    {
                        index = 0;
                    }
                    if (!bitArray[index])
                    {
                        bitArray.Set(index, true);
                        if (advancing)
                        {
                            offset = index + 1;
                            if (offset >= length)
                            {
                                offset = 0;
                            }
                        }
                        return (minValue + index);
                    }
                }
            }
            throw new OutOfResourceException();
        }
        
        /// <summary>
        /// Marks the specified value as used in the pool.
        /// </summary>
        public bool Claim(int value)
        {
            if (value < minValue || maxValue < value)
            {
                throw new ArgumentOutOfRangeException();
            }
            int index = value - minValue;
            lock (bitArray)
            {
                if (bitArray[index])
                {
                    return false;
                }
                bitArray.Set(index, true);
            }
            return true;
        }

        /// <summary>
        /// Returns the specified value to the pool.
        /// </summary>
        public void Release(int value)
        {
            if (value < minValue || maxValue < value)
            {
                throw new ArgumentOutOfRangeException();
            }
            int index = value - minValue;
            lock (bitArray)
            {
                if (bitArray[index])
                {
                    bitArray.Set(index, false);
                }
            }
        }
    }
}
