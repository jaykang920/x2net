// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Abstract base class for blocking event queue implementations.
    /// </summary>
    public abstract class EventQueue
    {
        public abstract int Length { get; }

        public void Close()
        {
            Close(null);
        }

        public abstract void Close(Event finalItem);

        public abstract Event Dequeue();

        public abstract void Enqueue(Event item);

        public abstract bool TryDequeue(out Event value);
    }
}
