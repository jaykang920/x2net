// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{

    public class ClientSessionRecoveryStrategy : SessionRecoveryStrategy
    {
        protected volatile bool recovering;

        public ClientLink ClientLink
        {
            get { return (ClientLink)base.Link; }
        }

        public override void Setup()
        {
            base.Setup();

            Link.EventFactory.Register(SessionResp.TypeId, SessionResp.New);
            Link.EventFactory.Register(SessionEnd.TypeId, SessionEnd.New);
        }

        public override void BeforeSessionSetup(LinkSession linkSession)
        {
            linkSession.SessionStrategy = new RecoverableSessionStrategy
            {
                Session = linkSession
            };

            var req = new SessionReq { _Transform = false };

            LinkSession currentSession = ClientLink.Session;
            if (!ReferenceEquals(currentSession, null) &&
                !String.IsNullOrEmpty(currentSession.Token))
            {
                req.Token = currentSession.Token;
                req.RxCounter = currentSession.RxCounter;
                req.TxCounter = currentSession.TxCounter;
                req.TxBuffered = currentSession.TxBuffered;

                Trace.Debug("{0} {1} rxC={2} txC={3} txCc={4}",
                    Link.Name, currentSession.Handle, currentSession.RxCounter,
                    currentSession.TxCounter, currentSession.TxCompleted);
            }

            linkSession.Send(req);
        }

        public override bool OnConnectError()
        {
            if (!recovering)
            {
                return false;
            }

            recovering = false;

            LinkSession deadSession = ClientLink.Session;

            deadSession.Release();

            SessionBasedLink.OnLinkSessionDisconnectedInternal(deadSession.Handle, deadSession);

            return true;
        }

        public override void OnInstantDisconnect(LinkSession session)
        {
            LinkSession currentSession = ClientLink.Session;
            if (!Object.ReferenceEquals(session, currentSession))
            {
                Trace.Warn("{0} gave up session recovery {1}", Link.Name, session.Handle);
                return;
            }

            recovering = true;

            ClientLink.Reconnect();
        }

        public override void OnSessionConnected(bool result, object context)
        {
            recovering = false;
        }

        public override void OnSessionRecovered(int handle, object context, int retransmission)
        {
            var session = (LinkSession)context;
            LinkSession oldSession = ClientLink.ResetSession(session);

            ((RecoverableSessionStrategy)session.SessionStrategy)
                .TakeOver(oldSession, retransmission);

            Trace.Debug("{0} {1} reset session {2}",
                Link.Name, session.Handle, session.Token);
        }

        internal void OnSessionResp(LinkSession session, SessionResp e)
        {
            LinkSession currentSession = ClientLink.Session;
            string sessionToken = null;
            if (!Object.ReferenceEquals(currentSession, null))
            {
                sessionToken = currentSession.Token;
            }

            // Save the session token from the server.
            session.Token = e.Token;

            Trace.Log("{0} {1} session token {2}",
                Link.Name, session.InternalHandle, e.Token);

            if (!String.IsNullOrEmpty(sessionToken))
            {
                if (sessionToken.Equals(e.Token))
                {
                    // Recovered from instant disconnection.
                    ((RecoverableSessionStrategy)session.SessionStrategy)
                        .InheritFrom(currentSession);

                    session.Send(new SessionAck
                    {
                        _Transform = false,
                        Recovered = true
                    });

                    OnLinkSessionRecovered(session.Handle, session, e.Retransmission);
                    return;
                }

                SessionBasedLink.OnLinkSessionDisconnectedInternal(currentSession.Handle, currentSession);
            }

            session.Send(new SessionAck { _Transform = false });

            SessionBasedLink.OnSessionSetup(session);
        }
    }

    public class ServerSessionRecoveryStrategy : SessionRecoveryStrategy
    {
        private Dictionary<string, LinkSession> recoverable;
        private Dictionary<int, Binder.Token> recoveryTokens;

        /// <summary>
        /// Gets or sets the session recovery timeout (in seconds).
        /// </summary>
        public int Timeout { get; set; }

        public ServerLink ServerLink
        {
            get { return (ServerLink)base.Link; }
        }

        public ServerSessionRecoveryStrategy()
        {
            recoverable = new Dictionary<string, LinkSession>();
            recoveryTokens = new Dictionary<int, Binder.Token>();

            Timeout = 10;  // 10 seconds by default
        }

        public override void Setup()
        {
            base.Setup();

            Link.EventFactory.Register(SessionReq.TypeId, SessionReq.New);
            Link.EventFactory.Register(SessionAck.TypeId, SessionAck.New);
            Link.EventFactory.Register(SessionEnd.TypeId, SessionEnd.New);
        }

        public override void Teardown()
        {
            lock (recoverable)
            {
                recoverable.Clear();
            }
            lock (recoveryTokens)
            {
                recoveryTokens.Clear();
            }
        }

        public override void BeforeSessionSetup(LinkSession linkSession)
        {
            linkSession.SessionStrategy = new RecoverableSessionStrategy
            {
                Session = linkSession
            };
        }

        public override void AfterSessionTeardown(LinkSession linkSession)
        {
            lock (recoverable)
            {
                recoverable.Remove(linkSession.Token);
            }
        }

        public override void OnInstantDisconnect(LinkSession session)
        {
            bool flag;
            // Ensure that the specified session is recoverable.
            LinkSession existing = null;
            lock (recoverable)
            {
                flag = recoverable.TryGetValue(session.Token, out existing);
            }
            if (!flag)
            {
                Trace.Info("{0} {1} unrecoverable session", Link.Name, session.Handle);

                SessionBasedLink.OnLinkSessionDisconnectedInternal(session.Handle, session);
                return;
            }
            else if (!Object.ReferenceEquals(session, existing))
            {
                Trace.Warn("{0} {1} gave up session recovery", Link.Name, session.Handle);
                return;
            }

            existing = null;
            ServerLink.TryGetSession(session.Handle, out existing);
            if (!Object.ReferenceEquals(session, existing))
            {
                Trace.Warn("{0} {1} gave up session recovery", Link.Name, session.Handle);
                return;
            }

            var e = new TimeoutEvent { Key = session };

            Binder.Token binderToken = Link.Flow.Subscribe(e, OnTimeout);
            TimeFlow.Default.Reserve(e, Timeout);
            lock (recoveryTokens)
            {
                recoveryTokens[session.Handle] = binderToken;
            }

            Trace.Log("{0} {1} started recovery timer", Link.Name, session.Handle);
        }


        public override void OnSessionRecovered(int handle, object context, int retransmission)
        {
            LinkSession oldSession;
            var session = (LinkSession)context;
            oldSession = ServerLink.ResetSession(handle, session);

            ((RecoverableSessionStrategy)session.SessionStrategy)
                .TakeOver(oldSession, retransmission);

            lock (recoverable)
            {
                recoverable[session.Token] = session;
            }
        }

        internal void OnSessionReq(LinkSession session, SessionReq e)
        {
            string clientToken = e.Token;
            bool flag = false;
            if (!String.IsNullOrEmpty(clientToken))
            {
                int incomingRetransmission = 0;
                int outgoingRetransmission = 0;
                LinkSession existing;
                lock (recoverable)
                {
                    flag = recoverable.TryGetValue(clientToken, out existing);
                }
                if (flag)
                {
                    Trace.Debug("{0} {1} rxC={2} txC={3} rxS={4} txS={5} txSc={6}",
                        Link.Name, existing.Handle, e.RxCounter, e.TxCounter,
                        existing.RxCounter, existing.TxCounter, existing.TxCompleted);

                    if (e.RxCounter < existing.TxCounter)
                    {
                        outgoingRetransmission = (int)(existing.TxCounter - e.RxCounter);
                        if (outgoingRetransmission > existing.TxBuffered)
                        {
                            flag = false;
                        }
                    }
                    if (e.TxCounter > existing.RxCounter)
                    {
                        incomingRetransmission = (int)(e.TxCounter - existing.RxCounter);
                        if (incomingRetransmission > e.TxBuffered)
                        {
                            flag = false;
                        }
                    }
                    if (!flag)
                    {
                        Trace.Warn("{0} {1} gave up session recovery",
                            Link.Name, existing.Handle);

                        SessionBasedLink.OnLinkSessionDisconnectedInternal(existing.Handle, existing);
                    }
                }
                if (flag)
                {
                    int handle;
                    lock (existing.SyncRoot)
                    {
                        handle = existing.Handle;
                    }

                    if (handle != 0)
                    {
                        lock (existing.SyncRoot)
                        {
                            CancelRecoveryTimer(handle);
                            ((RecoverableSessionStrategy)session.SessionStrategy)
                                .InheritFrom(existing);
                        }

                        session.Send(new SessionResp
                        {
                            _Transform = false,
                            Token = session.Token,
                            Retransmission = incomingRetransmission
                        });

                        OnLinkSessionRecovered(handle, session, outgoingRetransmission);
                        return;
                    }
                }
            }

            // Issue a new session token for the given session.
            session.Token = Guid.NewGuid().ToString("N");

            Trace.Debug("{0} {1} issued session token {2}",
                Link.Name, session.InternalHandle, session.Token);

            lock (recoverable)
            {
                recoverable[session.Token] = session;
            }

            session.Send(new SessionResp
            {
                _Transform = false,
                Token = session.Token
            });
        }

        internal void OnSessionAck(LinkSession session, SessionAck e)
        {
            if (!e.Recovered)
            {
                SessionBasedLink.OnSessionSetup(session);
            }
        }

        private void CancelRecoveryTimer(int handle)
        {
            bool found = false;
            Binder.Token binderToken;
            lock (recoveryTokens)
            {
                if (recoveryTokens.TryGetValue(handle, out binderToken))
                {
                    recoveryTokens.Remove(handle);
                    found = true;
                }
            }
            if (found)
            {
                Link.Flow.Unsubscribe(binderToken);
                TimeFlow.Default.Cancel(binderToken.Key);

                Trace.Log("{0} {1} canceled recovery timer", Link.Name, handle);
            }
        }

        void OnTimeout(TimeoutEvent e)
        {
            Link.Flow.Unsubscribe(e, OnTimeout);
            var session = e.Key as LinkSession;
            if (Object.ReferenceEquals(session, null))
            {
                return;
            }
            session.Release();

            Trace.Debug("{0} {1} session recovery timeout {1} {2}",
                Link.Name, session.Handle, session.Token);

            lock (recoveryTokens)
            {
                recoveryTokens.Remove(session.Handle);
            }

            SessionBasedLink.OnLinkSessionDisconnectedInternal(session.Handle, session);
        }
    }

    public class RecoverableSessionStrategy : SessionRecoveryStrategy.SubStrategy
    {
        private List<Event> queue;

        public RecoverableSessionStrategy()
        {
            queue = new List<Event>();
        }

        public override void Teardown()
        {
            queue.Clear();
        }

        public override void OnClose()
        {
            Session.Send(new SessionEnd { _Transform = false });
        }

        public override void OnDispose()
        {
            // Postpone disposing send buffers

            queue.Clear();
        }

        public override bool BeforeSend(Event e)
        {
            if (!Session.Disposed && !Session.Connected &&
                e.GetTypeId() > 0)  // not builtin events
            {
                Trace.Info("{0} {1} pre-establishment buffered {2}",
                    Session.Link.Name, Session.InternalHandle, e);
                queue.Add(e);
                return true;
            }
            return false;
        }

        public override bool Process(Event e)
        {
            int typeId = e.GetTypeId();
            if (Session.Connected)
            {
                if (typeId != (int)LinkEventType.SessionEnd)
                {
                    return false;
                }
                Session.Closing = true;
            }
            else
            {
                switch (typeId)
                {
                    case (int)LinkEventType.SessionReq:
                        if (Session.Polarity == false)
                        {
                            var server = (ServerLink)Session.Link;
                            ((ServerSessionRecoveryStrategy)server.SessionStrategy).OnSessionReq(Session, (SessionReq)e);
                        }
                        break;
                    case (int)LinkEventType.SessionResp:
                        if (Session.Polarity == true)
                        {
                            var client = (ClientLink)Session.Link;
                            ((ClientSessionRecoveryStrategy)client.SessionStrategy).OnSessionResp(Session, (SessionResp)e);
                        }
                        break;
                    case (int)LinkEventType.SessionAck:
                        if (Session.Polarity == false)
                        {
                            var server = (ServerLink)Session.Link;
                            ((ServerSessionRecoveryStrategy)server.SessionStrategy).OnSessionAck(Session, (SessionAck)e);
                        }
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        internal void InheritFrom(LinkSession oldSession)
        {
            lock (Session.SyncRoot)
            {
                Session.Handle = oldSession.Handle;
                Session.Token = oldSession.Token;

                Trace.Debug("{0} {1} session inheritance {2}",
                    Session.Link.Name, Session.Handle, Session.Token);

                Session.ChannelStrategy = oldSession.ChannelStrategy;
                oldSession.ChannelStrategy = null;
            }
        }

        internal void TakeOver(LinkSession oldSession, int retransmission)
        {
            if (retransmission == 0)
            {
                lock (Session.SyncRoot)
                {
                    lock (oldSession.SyncRoot)
                    {
                        if (oldSession.BuffersSending.Count != 0)
                        {
                            // Dispose them
                            for (int i = 0, count = oldSession.BuffersSending.Count; i < count; ++i)
                            {
                                oldSession.BuffersSending[i].Dispose();
                            }
                            oldSession.BuffersSending.Clear();
                        }
                        if (oldSession.BuffersSent.Count != 0)
                        {
                            // Dispose them
                            for (int i = 0, count = oldSession.BuffersSent.Count; i < count; ++i)
                            {
                                oldSession.BuffersSent[i].Dispose();
                            }
                            oldSession.BuffersSent.Clear();
                        }

                        Trace.Info("{0} {1} pre-establishment buffered {2}",
                            Session.Link.Name, Session.InternalHandle, queue.Count);

                        if (queue.Count != 0)
                        {
                            Session.EventsToSend.InsertRange(0, queue);
                            queue.Clear();
                        }
                        if (oldSession.EventsToSend.Count != 0)
                        {
                            Session.EventsToSend.InsertRange(0, oldSession.EventsToSend);
                            oldSession.EventsToSend.Clear();
                        }

                        Trace.Info("{0} {1} eventsToSend {2}", Session.Link.Name, Session.InternalHandle, Session.EventsToSend.Count);
                    }

                    Session.Connected = true;

                    if (Session.TxFlag || Session.EventsToSend.Count == 0)
                    {
                        return;
                    }

                    Session.TxFlag = true;
                }

                Session.BeginSend();
            }
            else
            {
                Monitor.Enter(Session.SyncRoot);
                try
                {
                    // Wait for any existing send to complete
                    while (Session.TxFlag)
                    {
                        Monitor.Exit(Session.SyncRoot);
                        Thread.Sleep(1);
                        Monitor.Enter(Session.SyncRoot);
                    }

                    Session.TxBufferList.Clear();
                    Session.LengthToSend = 0;
                    Session.BuffersSending.Clear();

                    lock (oldSession.SyncRoot)
                    {
                        List<SendBuffer> buffers1, buffers2;

                        if (oldSession.TxCounter == oldSession.TxCompleted)
                        {
                            buffers1 = oldSession.BuffersSending;
                            buffers2 = oldSession.BuffersSent;
                        }
                        else
                        {
                            buffers1 = oldSession.BuffersSent;
                            buffers2 = oldSession.BuffersSending;
                        }

                        int numBuffers1ToDispose = oldSession.TxBuffered - retransmission;
                        if (numBuffers1ToDispose > buffers1.Count)
                        {
                            numBuffers1ToDispose = buffers1.Count;
                        }
                        int numBuffers2ToDispose = buffers2.Count - retransmission;
                        if (numBuffers2ToDispose < 0)
                        {
                            numBuffers2ToDispose = 0;
                        }

                        int i;

                        for (i = 0; i < numBuffers1ToDispose; ++i)
                        {
                            buffers1[i].Dispose();
                        }
                        for (; i < buffers1.Count; ++i)
                        {
                            SendBuffer buffer = buffers1[i];
                            Session.BuffersSending.Add(buffer);
                            buffer.ListOccupiedSegments(Session.TxBufferList);
                            Session.LengthToSend += buffer.Length;
                        }
                        buffers1.Clear();

                        for (i = 0; i < numBuffers2ToDispose; ++i)
                        {
                            buffers2[i].Dispose();
                        }
                        for (; i < buffers2.Count; ++i)
                        {
                            SendBuffer buffer = buffers2[i];
                            Session.BuffersSending.Add(buffer);
                            buffer.ListOccupiedSegments(Session.TxBufferList);
                            Session.LengthToSend += buffer.Length;
                        }
                        buffers2.Clear();

                        Trace.Info("{0} {1} pre-establishment buffered {2}",
                            Session.Link.Name, Session.InternalHandle, queue.Count);

                        if (queue.Count != 0)
                        {
                            Session.EventsToSend.InsertRange(0, queue);
                            queue.Clear();
                        }
                        if (oldSession.EventsToSend.Count != 0)
                        {
                            Session.EventsToSend.InsertRange(0, oldSession.EventsToSend);
                            oldSession.EventsToSend.Clear();
                        }

                        Trace.Info("{0} {1} eventsToSend {2}", Session.Link.Name, Session.InternalHandle, Session.EventsToSend.Count);
                    }

                    Trace.Warn("{0} {1} retransmitting {2} events ({3} bytes)",
                        Session.Link.Name, Session.InternalHandle, retransmission, Session.LengthToSend);

                    Session.Connected = true;

                    Session.TxFlag = true;
                }
                finally
                {
                    Monitor.Exit(Session.SyncRoot);
                }

                //Session.IncrementTxCounter(retransmission);

                Session.SendInternal();
            }
        }
    }
}
