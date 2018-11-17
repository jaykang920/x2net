﻿// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// Helps code block scope based cleanup.
    /// </summary>
    public class Scope : IDisposable
    {
        protected Binding.Token? bindingToken;
        protected Event e;

        protected readonly DateTime startTime;
        protected double totalMillis;

        /// <summary>
        /// Gets or sets the binding token to be recovered on disposal.
        /// </summary>
        public Binding.Token? BindingToken
        {
            get { return bindingToken; }
            set { bindingToken = value; }
        }

        /// <summary>
        /// Gets or sets the event to be posted on disposal.
        /// </summary>
        public Event Event
        {
            get { return e; }
            set { e = value; }
        }

        /// <summary>
        /// A delegate type for hooking up Dispose notifications.
        /// </summary>
        public delegate void CleanupHandler(Event e);

        /// <summary>
        /// An event that clients can bind custom actions to be executed when
        /// this Scope object is disposed.
        /// </summary>
        public event CleanupHandler Cleanup;

        /// <summary>
        /// Initializes a new Scope object.
        /// </summary>
        public Scope()
        {
            startTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new Scope object with the specified event.
        /// </summary>
        public Scope(Event e)
        {
            startTime = DateTime.UtcNow;
            this.e = e;
        }

        /// <summary>
        /// Implements IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            if (Cleanup != null)
            {
                Cleanup(e);
            }

            if (bindingToken.HasValue &&
                !ReferenceEquals(bindingToken.Value.Key, null))
            {
                Flow.Bind(bindingToken.Value);
            }

            if (!ReferenceEquals(e, null))
            {
                Hub.Post(e);
            }

            if (!ReferenceEquals(Flow.CurrentFlow, null))
            {
                DateTime endTime = DateTime.UtcNow;
                totalMillis = (endTime - startTime).TotalMilliseconds;
                if ((int)totalMillis >= Flow.CurrentFlow.SlowScopeLogThreshold &&
                    Config.TraceLevel <= Flow.CurrentFlow.SlowScopeTraceLevel)
                {
                    Trace.Emit(Flow.CurrentFlow.SlowScopeTraceLevel,
                        "{0} slow scope {1:#,0}ms on {2}",
                        Flow.CurrentFlow.Name, totalMillis, e);
                }
            }

            OnDispose();
        }

        /// <summary>
        /// Overridden by subclasses to build up a cleanup chain along the
        /// inheritance hierarchy.
        /// </summary>
        protected virtual void OnDispose() { }
    }
}
