// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Common abstract base class for session-based links.
    /// </summary>
    public abstract class SessionBasedLink : Link
    {
        protected ReaderWriterLockSlim rwlock;

        /// <summary>
        /// A delegate type for hooking up event proprocess notifications.
        /// </summary>
        public delegate void PreprocessEventHandler(LinkSession session, Event e);

        /// <summary>
        /// An event that clients can use to be notified whenever a new event is
        /// ready for preprocessing.
        /// </summary>
        public event PreprocessEventHandler Preprocess;

        // Strategies

        /// <summary>
        /// Gets or sets the channel strategy object for this link.
        /// </summary>
        public ChannelStrategy ChannelStrategy { get; set; }
        /// <summary>
        /// Gets or sets the heartbeat strategy object for this link.
        /// </summary>
        public HeartbeatStrategy HeartbeatStrategy { get; set; }
        /// <summary>
        /// Gets or sets the session strategy object for this link.
        /// </summary>
        public SessionStrategy SessionStrategy { get; set; }

        /// <summary>
        /// Gets whether this link has active channel strategy or not.
        /// </summary>
        public bool HasChannelStrategy
        {
            get { return !ReferenceEquals(ChannelStrategy, null); }
        }
        /// <summary>
        /// Gets whether this link has active heartbeat strategy or not.
        /// </summary>
        public bool HasHeartbeatStrategy
        {
            get { return !ReferenceEquals(HeartbeatStrategy, null); }
        }
        /// <summary>
        /// Gets whether this link has active session strategy or not.
        /// </summary>
        public bool HasSessionStrategy
        {
            get { return !ReferenceEquals(SessionStrategy, null); }
        }

        static SessionBasedLink()
        {
            EventFactory.Global.Register(HandshakeReq.TypeId, HandshakeReq.New);
            EventFactory.Global.Register(HandshakeResp.TypeId, HandshakeResp.New);
            EventFactory.Global.Register(HandshakeAck.TypeId, HandshakeAck.New);
        }

        /// <summary>
        /// Initializes a new instance of the SessionBasedLink class.
        /// </summary>
        protected SessionBasedLink(string name)
            : base(name)
        {
            rwlock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Called internally when a new session creation attempt is completed.
        /// </summary>
        protected internal void OnLinkSessionConnectedInternal(bool result, object context)
        {
            Trace.Info("{0} connected {1} {2}", Name, result, context);

            if (result)
            {
                var session = (LinkSession)context;
                lock (session.SyncRoot)
                {
                    // Assign a new link session handle.
                    session.Handle = HandlePool.Acquire();
                }
                session.Connected = true;
            }

            OnSessionConnectedInternal(result, context);

            Hub.Post(new LinkSessionConnected {
                LinkName = Name,
                Result = result,
                Context = context
            });
        }

        /// <summary>
        /// Called internally when an existing link session is closed.
        /// </summary>
        internal void OnLinkSessionDisconnectedInternal(int handle, object context)
        {
            Trace.Info("{0} disconnected {1} {2}", Name, handle, context);

            if (handle == 0)
            {
                return;
            }

            // Release the link session handle.
            HandlePool.Release(handle);

            var session = (LinkSession)context;
            session.Connected = false;

            try
            {
                OnSessionDisconnectedInternal(handle, context);
            }
            catch (ObjectDisposedException)
            {
                // already disposed
                // TODO need to be improved
            }

            Hub.Post(new LinkSessionDisconnected {
                LinkName = Name,
                Handle = handle,
                Context = context
            });
        }

        internal protected void OnPreprocess(LinkSession session, Event e)
        {
            if (Preprocess != null)
            {
                Preprocess(session, e);
            }
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (HasChannelStrategy)
            {
                ChannelStrategy.Release();
            }

            rwlock.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called when a new session creation attempt is completed.
        /// </summary>
        protected virtual void OnSessionConnected(bool result, object context)
        {
        }

        protected abstract void OnSessionConnectedInternal(bool result, object context);

        /// <summary>
        /// Called when an existing link session is closed.
        /// </summary>
        protected virtual void OnSessionDisconnected(int handle, object context)
        {
        }

        protected abstract void OnSessionDisconnectedInternal(int handle, object context);

        /// <summary>
        /// Called when a new link session is ready for open.
        /// </summary>
        public void OnSessionSetup(LinkSession session)
        {
            if (HasChannelStrategy)
            {
                ChannelStrategy.InitiateHandshake(session);
            }
            else
            {
                OnLinkSessionConnectedInternal(true, session);
            }
        }

        /// <summary>
        /// <see cref="Case.SetupInternal"/>
        /// </summary>
        protected override void SetupInternal()
        {
            base.SetupInternal();

            Bind(new LinkSessionConnected { LinkName = Name },
                OnLinkSessionConnected);
            Bind(new LinkSessionDisconnected { LinkName = Name },
                OnLinkSessionDisconnected);

            if (HasChannelStrategy)
            {
                ChannelStrategy.Link = this;
                ChannelStrategy.Setup();
            }
            if (HasHeartbeatStrategy)
            {
                HeartbeatStrategy.Link = this;
                HeartbeatStrategy.Setup();
            }
            if (HasSessionStrategy)
            {
                SessionStrategy.Link = this;
                SessionStrategy.Setup();
            }

            if (HasHeartbeatStrategy)
            {
                Bind(Hub.HeartbeatEvent, OnHeartbeatEvent);
            }
        }

        /// <summary>
        /// <see cref="Case.TeardownInternal"/>
        /// </summary>
        protected override void TeardownInternal()
        {
            if (HasSessionStrategy)
            {
                SessionStrategy.Teardown();
                SessionStrategy.Link = null;
            }
            if (HasHeartbeatStrategy)
            {
                HeartbeatStrategy.Teardown();
                HeartbeatStrategy.Link = null;
            }
            if (HasChannelStrategy)
            {
                ChannelStrategy.Teardown();
                ChannelStrategy.Link = null;
            }

            base.TeardownInternal();
        }

        // LinkSessionConnected event handler
        private void OnLinkSessionConnected(LinkSessionConnected e)
        {
            OnSessionConnected(e.Result, e.Context);
        }

        // LinkSessionDisconnected event handler
        private void OnLinkSessionDisconnected(LinkSessionDisconnected e)
        {
            OnSessionDisconnected(e.Handle, e.Context);
        }

        void OnHeartbeatEvent(HeartbeatEvent e)
        {
            HeartbeatStrategy.OnHeartbeat();
        }
    }
}
