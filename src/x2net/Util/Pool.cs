// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// Thread-safe minimal generic object pool.
    /// </summary>
    public class Pool<T> where T : class
    {
        private Stack<T> store;
        private int capacity;
        private object syncRoot;

        /// <summary>
        /// Gets or sets the maximum number of objects that can be contained in
        /// the pool.
        /// </summary>
        public int Capacity
        {
            get { lock (syncRoot) { return capacity; } }
            set { lock (syncRoot) { capacity = value; } }
        }

        /// <summary>
        /// Gets the number of objects contained in the pool.
        /// </summary>
        public int Count
        {
            get { lock (syncRoot) { return store.Count; } }
        }

        /// <summary>
        /// Initializes a new instance of the Pool(T) class, without a capacity
        /// limit.
        /// </summary>
        public Pool() : this(0) { }

        /// <summary>
        /// Initializes a new instance of the Pool(T) class, with the specified
        /// maximum capacity.
        /// </summary>
        public Pool(int capacity)
        {
            store = new Stack<T>();
            this.capacity = capacity;
            syncRoot = new Object();
        }

        /// <summary>
        /// Tries to pop an object out of the pool.
        /// </summary>
        /// <returns>
        /// The object removed from the pool, or null if the pool is empty.
        /// </returns>
        public T Pop()
        {
            lock (syncRoot)
            {
                if (store.Count != 0)
                {
                    return store.Pop();
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to push the specified object into the pool.
        /// </summary>
        /// <remarks>
        /// If the pool has a non-zero capacity limit, the object may be dropped
        /// when the number of pooled objects reaches the capacity.
        /// </remarks>
        public void Push(T item)
        {
            if (Object.ReferenceEquals(item, null))
            {
                throw new ArgumentNullException();
            }
            lock (syncRoot)
            {
                if (capacity == 0 || store.Count < capacity)
                {
                    store.Push(item);
                }
            }
        }
    }
}
