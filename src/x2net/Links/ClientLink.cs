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
        protected LinkSession session;      // current link session

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

        static ClientLink()
        {
            EventFactory.Global.Register(SessionResp.TypeId, SessionResp.New);
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

        protected override void OnSessionRecoveredInternal(
            int handle, object context, int retransmission)
        {
            LinkSession oldSession;
            var session = (LinkSession)context;
            using (new WriteLock(rwlock))
            {
                oldSession = this.session;
                this.session = session;
            }

            session.TakeOver(oldSession, retransmission);

            Trace.Debug("{0} {1} reset session {2}",
                Name, session.Handle, session.Token);
        }

        internal void OnSessionResp(LinkSession session, SessionResp e)
        {
            LinkSession currentSession = Session;
            string sessionToken = null;
            if (!Object.ReferenceEquals(currentSession, null))
            {
                sessionToken = currentSession.Token;
            }

            // Save the session token from the server.
            session.Token = e.Token;

            Trace.Log("{0} {1} session token {2}",
                Name, session.InternalHandle, e.Token);

            if (!String.IsNullOrEmpty(sessionToken))
            {
                if (sessionToken.Equals(e.Token))
                {
                    // Recovered from instant disconnection.
                    session.InheritFrom(this.session);

                    session.Send(new SessionAck {
                        _Transform = false,
                        Recovered = true
                    });

                    OnLinkSessionRecoveredInternal(session.Handle, session, e.Retransmission);
                    return;
                }

                OnLinkSessionDisconnectedInternal(currentSession.Handle, currentSession);
            }

            session.Send(new SessionAck { _Transform = false });

            OnSessionSetup(session);
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

            if (SessionRecoveryEnabled)
            {
                SendSessionReq(session);
            }
            else
            {
                OnSessionSetup(session);
            }
        }

        private void SendSessionReq(LinkSession session)
        {
            var req = new SessionReq { _Transform = false };

            LinkSession currentSession = Session;
            if (!Object.ReferenceEquals(currentSession, null) &&
                !String.IsNullOrEmpty(currentSession.Token))
            {
                req.Token = currentSession.Token;
                req.RxCounter = currentSession.RxCounter;
                req.TxCounter = currentSession.TxCounter;
                req.TxBuffered = currentSession.TxBuffered;
            }

            session.Send(req);
        }
    }
}
