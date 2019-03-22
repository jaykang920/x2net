﻿// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

#if NET40

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace x2net
{
    public class ThreadPoolFlow
        : ThreadPoolFlow<ConcurrentEventQueue>
    {
        public ThreadPoolFlow() : base() { }

        public ThreadPoolFlow(string name) : base(name) { }
    }

    // Performance problem on Linux
    public class ThreadPoolFlow<T> : EventBasedFlow<T> where T : EventQueue, new()
    {
        protected Thread thread;

        public ThreadPoolFlow()
        {
            thread = null;
        }

        public ThreadPoolFlow(string name)
            : this()
        {
            this.name = name;
        }

        public override Flow Start()
        {
            lock (syncRoot)
            {
                if (thread == null)
                {
                    SetupInternal();
                    caseStack.Setup(this);
                    thread = new Thread(Run) {
                        Name = name
                    };
                    thread.Start();
                    queue.Enqueue(new FlowStart());
                }
            }
            return this;
        }

        public override void Stop()
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
            while (true)
            {
                Event e = queue.Dequeue();
                if (ReferenceEquals(e, null))
                {
                    break;
                }

                var task = new Task(Handle, e);
                task.Start();
            }
        }

        private void Handle(object o)
        {
            currentFlow = this;
            if (ReferenceEquals(equivalent, null))
            {
                equivalent = new EventEquivalent();
            }
            if (ReferenceEquals(handlerChain, null))
            {
                handlerChain = new List<Handler>();
            }

            Dispatch((Event)o);
        }
    }
}

#endif
