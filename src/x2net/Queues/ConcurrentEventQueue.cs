// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

// Proposed and contributed by @keedongpark

#if NET40

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Unbounded event queue based on .NET 4 concurrent (lock-free) queue.
    /// </summary>
    public class ConcurrentEventQueue : EventQueue
    {
        private ConcurrentQueue<Event> queue;
        private volatile bool closing;

        public override int Length { get { return queue.Count; } }

        public ConcurrentEventQueue()
        {
            queue = new ConcurrentQueue<Event>();
        }

        public override void Close(Event finalItem)
        {
            queue.Enqueue(finalItem);
            closing = true;
        }

        public override Event Dequeue()
        {
            Event result = null;

            while (!queue.TryDequeue(out result))
            {
                if (closing)
                {
                    break;
                }
                Thread.Sleep(1);
            }

            return result;
        }

        public override void Enqueue(Event item)
        {
            queue.Enqueue(item);
        }

        public override bool TryDequeue(out Event value)
        {
            return queue.TryDequeue(out value);
        }
    }
}

#endif
