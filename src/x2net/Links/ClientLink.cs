// Copyright (c) 2017-2019 Jae-jun Kang
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
            if (ReferenceEquals(currentSession, null))
            {
                Trace.Info("{0} dropped event {1}", Name, e);
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

            try
            {
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
            }
            catch (Exception e)
            {
                Trace.Debug(e.ToString());
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
                using (new UpgradeableReadLock(rwlock))
                {
                    var oldSession = this.session;
                    if (!ReferenceEquals(oldSession, null))
                    {
                        oldSession.Close();
                    }
                    using (new WriteLock(rwlock))
                    {
                        this.session = session;
                    }
                }
            }
        }

        protected override void OnSessionDisconnectedInternal(int handle, object context)
        {
            using (new WriteLock(rwlock))
            {
                if (!ReferenceEquals(session, null) && ReferenceEquals(session, context))
                {
                    session = null;
                }
            }
        }
    }
}
