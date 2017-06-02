// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Abstract base class for session-based links.
    /// </summary>
    public abstract class SessionBasedLink : Link
    {
        protected ReaderWriterLockSlim rwlock;

        private volatile bool sessionRecoveryEnabled;

        /// <summary>
        /// Gets or sets a boolean value inidicating whether this link supports
        /// automatic session recovery on instant disconnection.
        /// </summary>
        public bool SessionRecoveryEnabled
        {
            get { return sessionRecoveryEnabled; }
            set { sessionRecoveryEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the session recovery timeout (in seconds).
        /// </summary>
        public int SessionRecoveryTimeout { get; set; }

        /// <summary>
        /// A delegate type for hooking up event proprocess notifications.
        /// </summary>
        public delegate void PreprocessEventHandler(LinkSession session, Event e);

        /// <summary>
        /// An event that clients can use to be notified whenever a new event is
        /// ready for preprocessing.
        /// </summary>
        public event PreprocessEventHandler Preprocess;

        static SessionBasedLink()
        {
            EventFactory.Register(HandshakeReq.TypeId, HandshakeReq.New);
            EventFactory.Register(HandshakeResp.TypeId, HandshakeResp.New);
            EventFactory.Register(HandshakeAck.TypeId, HandshakeAck.New);

            EventFactory.Register(SessionEnd.TypeId, SessionEnd.New);
        }

        /// <summary>
        /// Initializes a new instance of the SessionBasedLink class.
        /// </summary>
        protected SessionBasedLink(string name)
            : base(name)
        {
            rwlock = new ReaderWriterLockSlim();

            SessionRecoveryTimeout = 10;  // 10 seconds by default
        }

        /// <summary>
        /// Called internally when a new session creation attempt is completed.
        /// </summary>
        internal void OnLinkSessionConnectedInternal(bool result, object context)
        {
            Log.Info("{0} connected {1} {2}", Name, result, context);

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
            Log.Info("{0} disconnected {1} {2}", Name, handle, context);

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

        internal void OnLinkSessionRecoveredInternal(
            int handle, object context, int retransmission)
        {
            OnSessionRecoveredInternal(handle, context, retransmission);

            Hub.Post(new LinkSessionRecovered {
                LinkName = Name,
                Handle = handle,
                Context = context
            });

            Log.Info("{0} recovered {1} {2}", Name, handle, context);
        }

        internal abstract void OnInstantDisconnect(LinkSession session);

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

            rwlock.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived class to initiate a buffer transform handshake.
        /// </summary>
        protected void InitiateHandshake(LinkSession session)
        {
            if (Object.ReferenceEquals(BufferTransform, null))
            {
                return;
            }

            var bufferTransform = (IBufferTransform)BufferTransform.Clone();
            session.BufferTransform = bufferTransform;
            LinkWaitHandlePool.Acquire(session.InternalHandle).Set();

            session.Send(new HandshakeReq {
                _Transform = false,
                Data = bufferTransform.InitializeHandshake()
            });
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
        /// Called when an existing link session is recovered.
        /// </summary>
        protected virtual void OnSessionRecovered(int handle, object context)
        {
        }

        protected virtual void OnSessionRecoveredInternal(
            int handle, object context, int retransmission)
        {
        }

        /// <summary>
        /// Called when a new link session is ready for open.
        /// </summary>
        protected void OnSessionSetup(LinkSession session)
        {
            if (BufferTransform != null)
            {
                InitiateHandshake(session);
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
            Bind(new LinkSessionConnected { LinkName = Name },
                OnLinkSessionConnected);
            Bind(new LinkSessionDisconnected { LinkName = Name },
                OnLinkSessionDisconnected);
            Bind(new LinkSessionRecovered { LinkName = Name },
                OnLinkSessionRecovered);

            base.SetupInternal();
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

        // LinkSessionRecovered event handler
        private void OnLinkSessionRecovered(LinkSessionRecovered e)
        {
            OnSessionRecovered(e.Handle, e.Context);
        }
    }
}
