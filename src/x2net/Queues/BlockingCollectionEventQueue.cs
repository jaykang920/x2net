// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

// Proposed and contributed by @barowa

#if NET40

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Unbounded event queue based on .NET 4 concurrent (lock-free) queue.
    /// </summary>
    public class BlockingCollectionEventQueue : EventQueue
    {
        private BlockingCollection<Event> queue;
        private volatile bool closing;
        private CancellationTokenSource ts;

        public override int Length { get { return queue.Count; } }

        public BlockingCollectionEventQueue()
        {
            queue = new BlockingCollection<Event>();
            ts = new CancellationTokenSource();
        }

        public override void Close(Event finalItem)
        {
            queue.Add(finalItem);
            closing = true;
            ts.Cancel();
        }

        public override Event Dequeue()
        {
            Event result = null;

            if (closing)
            {
                queue.TryTake(out result, 0, CancellationToken.None);
            }
            else
            {
                try
                {
                    queue.TryTake(out result, Timeout.Infinite, ts.Token);
                }
                catch (OperationCanceledException)
                {
                    queue.TryTake(out result, 0, CancellationToken.None);
                }
            }

            return result;
        }

        public override void Enqueue(Event item)
        {
            queue.Add(item);
        }

        public override bool TryDequeue(out Event value)
        {
            return queue.TryTake(out value, 0, CancellationToken.None);
        }
    }
}

#endif
