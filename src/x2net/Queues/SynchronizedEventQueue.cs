// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Unbounded event queue based on lock and Monitor Wait/Pulse.
    /// </summary>
    public class SynchronizedEventQueue : EventQueue
    {
        private Queue<Event> queue;
        private bool closing;

        public override int Length
        {
            get
            {
                lock (queue)
                {
                    return queue.Count;
                }
            }
        }

        public SynchronizedEventQueue()
        {
            queue = new Queue<Event>();
        }

        public override void Close(Event finalItem)
        {
            lock (queue)
            {
                queue.Enqueue(finalItem);
                closing = true;
                Monitor.PulseAll(queue);
            }
        }

        public override Event Dequeue()
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (closing)
                    {
                        return null;
                    }
                    Monitor.Wait(queue);
                }
                return queue.Dequeue();
            }
        }

        public override void Enqueue(Event item)
        {
            lock (queue)
            {
                if (!closing)
                {
                    queue.Enqueue(item);
                    if (queue.Count == 1)
                    {
                        Monitor.Pulse(queue);
                    }
                }
            }
        }

        public override bool TryDequeue(out Event value)
        {
            lock (queue)
            {
                if (queue.Count == 0)
                {
                    value = null;
                    return false;
                }
                value = queue.Dequeue();
                return true;
            }
        }
    }
}
