// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    public class MultiThreadFlow
#if NET40
        : MultiThreadFlow<ConcurrentEventQueue>
#else
        : MultiThreadFlow<SynchronizedEventQueue>
#endif
    {
        public MultiThreadFlow() : base() { }

        public MultiThreadFlow(int numThreads) : base(numThreads) { }

        public MultiThreadFlow(string name) : base(name) { }

        public MultiThreadFlow(string name, int numThreads) : base(name, numThreads) { }
    }

    public class MultiThreadFlow<T> : EventBasedFlow<T> where T : EventQueue, new()
    {
        protected List<Thread> threads;
        protected int numThreads;

        public ThreadPriority Priority { get; set; }

        public MultiThreadFlow()
            : this(Environment.ProcessorCount)
        {
        }

        public MultiThreadFlow(int numThreads)
        {
            threads = new List<Thread>();
            this.numThreads = numThreads;

            Priority = ThreadPriority.Normal;
        }

        public MultiThreadFlow(string name)
            : this(name, Environment.ProcessorCount)
        {
        }

        public MultiThreadFlow(string name, int numThreads)
            : this(numThreads)
        {
            this.name = name;
        }

        public override Flow Start()
        {
            lock (syncRoot)
            {
                if (threads.Count == 0)
                {
                    SetupInternal();
                    caseStack.Setup(this);
                    for (int i = 0; i < numThreads; ++i)
                    {
                        Thread thread = new Thread(Run) {
                            Name = String.Format("{0} {1}", name, i + 1),
                            Priority = Priority
                        };
                        threads.Add(thread);
                        thread.Start();
                    }
                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void Stop()
        {
            lock (syncRoot)
            {
                if (threads.Count == 0)
                {
                    return;
                }
                queue.Close(new FlowStop());
                foreach (Thread thread in threads)
                {
                    if (thread != null)
                    {
                        thread.Join();
                    }
                }
                threads.Clear();

                caseStack.Teardown(this);
                TeardownInternal();
            }
        }

        private void Run()
        {
            currentFlow = this;
            equivalent = new EventEquivalent();
            handlerChain = new List<Handler>();

            while (true)
            {
                Event e = queue.Dequeue();
                if (ReferenceEquals(e, null))
                {
                    break;
                }
                Dispatch(e);
            }

            handlerChain = null;
            equivalent = null;
            currentFlow = null;
        }
    }
}
