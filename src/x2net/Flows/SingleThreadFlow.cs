// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    public class SingleThreadFlow
#if NET40
        : SingleThreadFlow<ConcurrentEventQueue>
#else
        : SingleThreadFlow<SynchronizedEventQueue>
#endif
    {
        public SingleThreadFlow() : base() { }

        public SingleThreadFlow(string name) : base(name) { }
    }

    public class SingleThreadFlow<T> : EventBasedFlow<T> where T : EventQueue, new()
    {
        protected Thread thread;

        public ThreadPriority Priority { get; set; }

        public SingleThreadFlow()
        {
            thread = null;

            Priority = ThreadPriority.Normal;
        }

        public SingleThreadFlow(string name)
            : this()
        {
            this.name = name;
        }

        public override Flow Startup()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    SetupInternal();
                    caseStack.Setup(this);
                    thread = new Thread(Run) {
                        Name = name,
                        Priority = Priority
                    };
                    thread.Start();
                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void Shutdown()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    return;
                }
                queue.Close(new FlowStop());
                thread.Join();
                thread = null;

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
