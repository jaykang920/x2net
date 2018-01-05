// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// Represents a read-only collection in which each item of type T can be
    /// accessed with an integer index.
    /// </summary>
    public interface IIndexable<T>
    {
        /// <summary>
        /// Gets the length of this collection.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets the value of the item at the specified index.
        /// </summary>
        T this[int index] { get; }
    }

    /// <summary>
    /// Provides an offset-based indexer for the underlying IIndexable(T) object.
    /// </summary>
    public struct Capo<T> : IIndexable<T>
    {
        private IIndexable<T> indexable;
        private readonly int offset;

        /// <summary>
        /// Gets the effective length of this offset-based window.
        /// </summary>
        public int Length { get { return (indexable.Length - offset); } }

        /// <summary>
        /// Initializes a new Capo(T) value with the specified underlying
        /// IIndexable(T) object and the base offset.
        /// </summary>
        public Capo(IIndexable<T> indexable, int offset)
        {
            // Never throws.
            this.indexable = indexable;
            this.offset = offset;
        }
            
        /// <summary>
        /// Gets the value of the item at the actual index of (offset + index).
        /// </summary>
        public T this[int index]
        {
            get
            {
                int actualIndex = offset + index;
                // Just return a default value on out-of-range condition,
                // instead of throwing an exception.
                if (actualIndex < 0 || indexable.Length <= actualIndex)
                {
                    return default(T);
                }
                return indexable[actualIndex];
            }
        }
    }
}
