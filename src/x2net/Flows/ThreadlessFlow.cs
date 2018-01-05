// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }

    public class ThreadlessFlow<Q> : EventBasedFlow<Q> where Q : EventQueue, new()
    {
        protected bool running;

        public ThreadlessFlow()
        {
        }

        public override Flow Startup()
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

                    running = true;

                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void Shutdown()
        {
            lock (syncRoot)
            {
                if (!running)
                {
                    return;
                }
                queue.Close(new FlowStop());
                running = false;

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
            if (Object.ReferenceEquals(e, null))
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
            int n = 0;
            bool shouldCopy = !Object.ReferenceEquals(events, null);
            while (true)
            {
                Event e;
                if (!queue.TryDequeue(out e))
                {
                    break;
                }

                Dispatch(e);

                if (shouldCopy)
                {
                    events.Add(e);
                }
                ++n;
            }
            return n;
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
