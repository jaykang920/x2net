// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// Common base class for single-session client links.
    /// </summary>
    public abstract class ClientLink : SessionBasedLink
    {
        /// <summary>
        /// Current link session.
        /// </summary>
        protected LinkSession session;

        /// <summary>
        /// Gets the current link session.
        /// </summary>
        public LinkSession Session
        {
            get
            {
                using (new ReadLock(rwlock))
                {
                    return session;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ClientLink class.
        /// </summary>
        protected ClientLink(string name)
            : base(name)
        {
            Diag = new Diagnostics();
        }

        /// <summary>
        /// Tries to reconnect to the last successful remote address.
        /// </summary>
        public virtual void Reconnect() { }

        /// <summary>
        /// Sends out the specified event through this link channel.
        /// </summary>
        public override void Send(Event e)
        {
            LinkSession currentSession = Session;
            if (Object.ReferenceEquals(currentSession, null))
            {
                Trace.Warn("{0} dropped event {1}", Name, e);
                return;
            }
            currentSession.Send(e);
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            LinkSession session = null;
            using (new WriteLock(rwlock))
            {
                if (this.session != null)
                {
                    session = this.session;
                    this.session = null;
                }
            }
            if (session != null)
            {
                session.Close();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived link class on a successful connect. 
        /// </summary>
        protected virtual void OnConnectInternal(LinkSession session)
        {
            session.Polarity = true;
            InitiateSession(session);
        }

        protected override void OnSessionConnectedInternal(bool result, object context)
        {
            if (result)
            {
                var session = (LinkSession)context;
                using (new WriteLock(rwlock))
                {
                    this.session = session;
                }

                Trace.Debug("{0} {1} set session {2}",
                    Name, session.Handle, session.Token);
            }
        }

        protected override void OnSessionDisconnectedInternal(int handle, object context)
        {
            using (new WriteLock(rwlock))
            {
                if (!Object.ReferenceEquals(session, null))
                {
                    Trace.Debug("{0} {1} cleared session {2}",
                        Name, session.Handle, session.Token);

                    session = null;
                }
            }
        }
    }
}
