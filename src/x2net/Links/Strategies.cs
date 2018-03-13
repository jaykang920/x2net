// Copyright (c) 2017, 2018 Jae-jun Kang
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
        public Link Link { get; set; }

        public virtual void Setup() { }

        public virtual void Teardown() { }
    }

    /// <summary>
    /// Common abstract base class for session-based link strategies.
    /// </summary>
    public abstract class SessionBasedLinkStrategy : LinkStrategy
    {
        public SessionBasedLink SessionBasedLink
        {
            get { return (SessionBasedLink)base.Link; }
        }

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

        public virtual void Setup() { }

        public virtual void Teardown() { }

        public virtual void OnClose() { }

        public virtual void OnDispose() { }
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

    /// <summary>
    /// Common abstract base class for session management strategies.
    /// </summary>
    public abstract class SessionStrategy : SessionBasedLinkStrategy
    {
        public abstract void OnInstantDisconnect(LinkSession linkSession);

        public virtual void OnSessionConnected(bool result, object context) { }

        public virtual bool OnConnectError() { return false; }

        public abstract class SubStrategy : LinkSessionStrategy
        {
            public virtual bool BeforeSend(Event e) { return false; }
        }
    }
}
