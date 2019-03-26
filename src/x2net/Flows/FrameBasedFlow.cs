﻿// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Utility class to handle time information within a frame-based flow.
    /// </summary>
    public class Time
    {
        public const long TicksInMillisecond = 10000;  // 100-nanosecond tick
        public const long TicksInSecond = 1000 * TicksInMillisecond;

        private long startTicks;
        private long lastTicks;
        private long currentTicks;
        private long deltaTicks;

        /// <summary>
        /// Gets the number of ticks it took to complete the last frame.
        /// </summary>
        public long DeltaTicks { get { return deltaTicks; } }

        /// <summary>
        /// Gets the time in seconds it took to complete the last frame.
        /// </summary>
        public double DeltaTime
        {
            get { return new TimeSpan(deltaTicks).TotalSeconds; }
        }

        /// <summary>
        /// Number of ticks representing the start UTC DateTime of the current frame.
        /// </summary>
        public long CurrentTicks { get { return currentTicks; } }

        /// <summary>
        /// Gets the start local DateTime of the current frame.
        /// </summary>
        public DateTime Now { get { return new DateTime(currentTicks).ToLocalTime(); } }

        /// <summary>
        /// Gets the start UTC DateTime of the current frame.
        /// </summary>
        public DateTime UtcNow { get { return new DateTime(currentTicks); } }

        public void Initialize()
        {
            startTicks = DateTime.UtcNow.Ticks;
            lastTicks = startTicks;
            currentTicks = lastTicks;
        }

        public void BeforeUpdate()
        {
            currentTicks = DateTime.UtcNow.Ticks;
            deltaTicks = currentTicks - lastTicks;
        }

        public void AfterUpdate()
        {
            lastTicks = currentTicks;
        }
    }

    /// <summary>
    /// Abstract base class for frame-based (looping) execution flows.
    /// </summary>
    public abstract class FrameBasedFlow<T> : Flow where T : EventQueue, new()
    {
        protected T queue;
        protected readonly object syncRoot;
        protected Thread thread;

        private volatile bool shouldStop;

        public long Resolution { get; set; }

        public Time Time { get; private set; }

        protected FrameBasedFlow() : this(false)
        {
        }

        protected FrameBasedFlow(bool withQueue)
        {
            if (withQueue)
            {
                queue = new T();
            }
            syncRoot = new Object();
            thread = null;

            // Default resolution is 15.625ms (64 frame/sec)
            Resolution = 156250;  // 15.625 * Time.TicksInMillisecond

            Time = new Time();
        }
        
        public override void Feed(Event e)
        {
            if (ReferenceEquals(queue, null))
            {
                return;
            }
            queue.Enqueue(e);
        }

        public override Flow Start()
        {
            lock (syncRoot)
            {
                if ((object)thread == null)
                {
                    SetupInternal();
                    caseStack.Setup(this);
                    thread = new Thread(this.Run) {
                        Name = name
                    };
                    thread.Start();
                    if (queue != null)
                    {
                        queue.Enqueue(new FlowStart());
                    }
                }
            }
            return this;
        }

        public override void Stop()
        {
            lock (syncRoot)
            {
                if ((object)thread == null)
                {
                    return;
                }
                shouldStop = true;
                if (queue != null)
                {
                    queue.Close(new FlowStop());
                }
                thread.Join();
                thread = null;

                caseStack.Teardown(this);
                TeardownInternal();
            }
        }

        protected override void OnHeartbeat()
        {
            if ((object)queue == null)
            {
                return;
            }
            int length = queue.Length;
            if (length >= LongQueueLogThreshold)
            {
                Trace.Emit(LongQueueTraceLevel, "{0} queue length {1}", name, length);
            }
        }

        private void Run()
        {
            currentFlow = this;
            if ((object)queue != null)
            {
                equivalent = new EventEquivalent();
                handlerChain = new List<Handler>();
            }

            BeginInternal();

            while (!shouldStop)
            {
                UpdateInternal();

                if (queue != null)
                {
                    while ((DateTime.UtcNow.Ticks - Time.CurrentTicks) < Resolution)
                    {
                        Event e;
                        if (queue.TryDequeue(out e))
                        {
                            Dispatch(e);

                            if (e.GetTypeId() == (int)BuiltinEventType.FlowStop)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (shouldStop)
                            {
                                break;
                            }
                            else
                            {
                                Thread.Sleep(1);
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    var tickDelta = DateTime.UtcNow.Ticks - Time.CurrentTicks;
                    var delay = (tickDelta < Resolution ?
                        (int)((Resolution - tickDelta) / Time.TicksInMillisecond) : 0);
                    Thread.Sleep(delay);
                }
            }

            End();

            if (queue != null)
            {
                handlerChain = null;
                equivalent = null;
            }
            currentFlow = null;
        }

        private void BeginInternal()
        {
            Time.Initialize();

            Begin();
        }

        private void UpdateInternal()
        {
            Time.BeforeUpdate();

            Update();

            Time.AfterUpdate();
        }

        protected virtual void Begin() { }
        protected virtual void End() { }

        protected abstract void Update();
    }
}
