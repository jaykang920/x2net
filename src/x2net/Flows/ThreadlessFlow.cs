// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    public class ThreadlessFlow
#if NET40
        : ThreadlessFlow<ConcurrentEventQueue>
#else
        : ThreadlessFlow<SynchronizedEventQueue>
#endif
    {
        public ThreadlessFlow() : base() { }

        public ThreadlessFlow(string name) : base(name) { }
    }

    public class ThreadlessFlow<Q> : EventBasedFlow<Q> where Q : EventQueue, new()
    {
        protected bool running;
        protected List<Event> dequeued;

        public ThreadlessFlow()
        {
        }

        public ThreadlessFlow(string name)
            : this()
        {
            this.name = name;
        }

        public override Flow Start()
        {
            lock (syncRoot)
            {
                if (!running)
                {
                    SetupInternal();
                    caseStack.Setup(this);

                    currentFlow = this;
                    equivalent = new EventEquivalent();
                    handlerChain = new List<Handler>();

                    dequeued = new List<Event>();

                    running = true;

                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void Stop()
        {
            lock (syncRoot)
            {
                if (!running)
                {
                    return;
                }
                queue.Close(new FlowStop());
                running = false;

                dequeued = null;

                handlerChain = null;
                equivalent = null;
                currentFlow = null;

                caseStack.Teardown(this);
                TeardownInternal();
            }
        }

        public void Dispatch()
        {
            Event e = queue.Dequeue();
            if (ReferenceEquals(e, null))
            {
                return;
            }
            Dispatch(e);
        }

        public Event TryDispatch()
        {
            Event e;
            if (queue.TryDequeue(out e))
            {
                Dispatch(e);
                return e;
            }
            return null;
        }

        public int TryDispatchAll()
        {
            return TryDispatchAll(null);
        }

        public int TryDispatchAll(List<Event> events)
        {
            Event e;

            dequeued.Clear();
            while (queue.TryDequeue(out e))
            {
                dequeued.Add(e);
            }
            int count = dequeued.Count;
            if (count != 0)
            {
                if (!ReferenceEquals(events, null))
                {
                    events.AddRange(dequeued);
                }
                for (int i = 0; i < count; ++i)
                {
                    Dispatch(dequeued[i]);
                }
                dequeued.Clear();
            }
            return count;
        }

        public bool TryDequeue(out Event e)
        {
            return queue.TryDequeue(out e);
        }

        /// <summary>
        /// Wait for a single event of type (T) with timeout in seconds.
        /// </summary>
        public bool Wait<T>(T expected, out T actual, double seconds)
            where T : Event
        {
            return Wait(expected, out actual, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Wait for a single event of type (T) with timeout.
        /// </summary>
        public bool Wait<T>(T expected, out T actual, TimeSpan timeout)
            where T : Event
        {
            actual = null;
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            while (stopWatch.Elapsed < timeout)
            {
                Event dequeued;
                if (queue.TryDequeue(out dequeued))
                {
                    Dispatch(dequeued);

                    if (expected.Equivalent(dequeued))
                    {
                        actual = (T)dequeued;
                        return true;
                    }
                }

                Thread.Sleep(1);
            }
            return false;
        }

        /// <summary>
        /// Wait for multiple events with timeout in seconds.
        /// </summary>
        public bool Wait(double seconds, out Event[] actual, params Event[] expected)
        {
            return Wait(TimeSpan.FromSeconds(seconds), out actual, expected);
        }

        /// <summary>
        /// Wait for multiple events with timeout.
        /// </summary>
        public bool Wait(TimeSpan timeout, out Event[] actual, params Event[] expected)
        {
            int count = 0;
            actual = new Event[expected.Length];
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            while (stopWatch.Elapsed < timeout)
            {
                Event dequeued;
                if (queue.TryDequeue(out dequeued))
                {
                    Dispatch(dequeued);

                    for (int i = 0; i < expected.Length; ++i)
                    {
                        if (actual[i] == null && expected[i].Equivalent(dequeued))
                        {
                            actual[i] = dequeued;
                            if (++count >= expected.Length)
                            {
                                return true;
                            }
                            break;
                        }
                    }
                }

                Thread.Sleep(1);
            }
            return false;
        }
    }
}
