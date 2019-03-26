﻿// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;

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
