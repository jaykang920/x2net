// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Common abstract base class for link strategies.
    /// </summary>
    public abstract class LinkStrategy
    {
        /// <summary>
        /// Gets or sets the link object associated with this link strategy.
        /// </summary>
        public Link Link { get; set; }

        /// <summary>
        /// Called when the associated link is initialized.
        /// </summary>
        public virtual void Setup() { }

        /// <summary>
        /// Called when the associated link is cleaned up.
        /// </summary>
        public virtual void Teardown() { }
    }

    /// <summary>
    /// Common abstract base class for session-based link strategies.
    /// </summary>
    public abstract class SessionBasedLinkStrategy : LinkStrategy
    {
        /// <summary>
        /// Called immediately once a new link session object is created.
        /// </summary>
        public virtual void BeforeSessionSetup(LinkSession linkSession) { }

        /// <summary>
        /// Called once an existing link session object is shut down.
        /// </summary>
        public virtual void AfterSessionTeardown(LinkSession linkSession) { }
    }

    /// <summary>
    /// Common abstract base class for link session strategies.
    /// </summary>
    public abstract class LinkSessionStrategy
    {
        public LinkSession Session { get; set; }

        public virtual bool Process(Event e) { return false; }
    }

    /// <summary>
    /// Common abstract base class for communication channel strategies.
    /// </summary>
    public abstract class ChannelStrategy : SessionBasedLinkStrategy
    {
        public virtual void InitiateHandshake(LinkSession session) { }

        public virtual void Release() { }

        public abstract class SubStrategy : LinkSessionStrategy
        {
            public virtual void Release() { }

            public virtual bool BeforeSend(Buffer buffer, int length) { return false; }

            public virtual void AfterReceive(Buffer buffer, int length) { }
        }
    }

    /// <summary>
    /// Common abstract base class for heartbeat strategies.
    /// </summary>
    public abstract class HeartbeatStrategy : SessionBasedLinkStrategy
    {
        public virtual void OnHeartbeat() { }

        /// <summary>
        /// Link session strategy
        /// </summary>
        public abstract class SubStrategy : LinkSessionStrategy
        {
            private volatile bool marked;

            public bool Marked
            {
                get { return marked; }
                set { marked = value; }
            }

            public virtual bool OnHeartbeat() { return false; }

            public virtual void OnReceive() { }

            public virtual void OnSend(Event e) { }
        }
    }
}
