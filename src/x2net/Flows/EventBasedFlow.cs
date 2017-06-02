// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Abstract base class for event-based (waiting) execution flows.
    /// </summary>
    public abstract class EventBasedFlow<T> : Flow where T : EventQueue, new()
    {
        protected T queue;
        protected readonly object syncRoot;

        protected EventBasedFlow()
        {
            queue = new T();
            syncRoot = new Object();
        }

        public override void Feed(Event e)
        {
            queue.Enqueue(e);
        }

        protected override void OnHeartbeat()
        {
            int length = queue.Length;
            if (length >= LongQueueLogThreshold)
            {
                Log.Emit(LongQueueLogLevel, "{0} queue length {1}", name, length);
            }
        }
    }
}
