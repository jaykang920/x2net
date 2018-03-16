// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// Common base class for multi-session server links.
    /// </summary>
    public abstract class ServerLink : SessionBasedLink
    {
        /// <summary>
        /// Searchable set of active sessions.
        /// </summary>
        protected SortedList<int, LinkSession> sessions;

        public int SessionCount
        {
            get
            {
                using (new ReadLock(rwlock))
                {
                    return sessions.Count;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ServerLink class.
        /// </summary>
        protected ServerLink(string name)
            : base(name)
        {
            sessions = new SortedList<int, LinkSession>();

            Diag = new Diagnostics();
        }

        /// <summary>
        /// Broadcasts the specified event to all the connected clients.
        /// </summary>
        public void Broadcast(Event e)
        {
            List<LinkSession> snapshot = TakeSessionsSnapshot();
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].Send(e);
            }
        }

        public LinkSession ResetSession(int handle, LinkSession session)
        {
            LinkSession existing;
            using (new WriteLock(rwlock))
            {
                existing = sessions[handle];
                sessions[handle] = session;
            }
            return existing;
        }

        /// <summary>
        /// Sends out the specified event through this link channel.
        /// </summary>
        public override void Send(Event e)
        {
            LinkSession session;
            using (new ReadLock(rwlock))
            {
                if (!sessions.TryGetValue(e._Handle, out session))
                {
                    return;
                }
            }
            session.Send(e);
        }

        public bool TryGetSession(int handle, out LinkSession session)
        {
            using (new ReadLock(rwlock))
            {
                return sessions.TryGetValue(handle, out session);
            }
        }

        protected override void OnSessionConnectedInternal(bool result, object context)
        {
            if (result)
            {
                var session = (LinkSession)context;
                using (new WriteLock(rwlock))
                {
                    sessions.Add(session.Handle, session);
                }
            }
        }

        protected override void OnSessionDisconnectedInternal(int handle, object context)
        {
            var session = (LinkSession)context;
            using (new WriteLock(rwlock))
            {
                sessions.Remove(handle);
            }
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            // Closes all the active sessions
            List<LinkSession> snapshot = TakeSessionsSnapshot();
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].Close();
            }

            using (new WriteLock(rwlock))
            {
                sessions.Clear();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Called by a derived link class on a successful accept.
        /// </summary>
        protected virtual bool OnAcceptInternal(LinkSession session)
        {
            InitiateSession(session);
            return true;
        }

        public List<LinkSession> TakeSessionsSnapshot()
        {
            var result = new List<LinkSession>(sessions.Count);
            using (new ReadLock(rwlock))
            {
                var values = sessions.Values;
                for (int i = 0, count = values.Count; i < count; ++i)
                {
                    result.Add(values[i]);
                }
            }
            return result;
        }
    }
}
