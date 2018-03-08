// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    public abstract class KeepaliveStrategy : HeartbeatStrategy
    {
        /// <summary>
        /// Gets or sets a boolean value indicating whether this link checks
        /// for incomming keepalive events
        /// </summary>
        public bool IncomingHeartbeatEnabled { get; set; }
        /// <summary>
        /// Gets or sets a boolean value indicating whether this link emits
        /// outgoing keepalive events.
        /// </summary>
        public bool OutgoingHeartbeatEnabled { get; set; }

        public int ThresholdFailuerCount { get; set; }

        protected KeepaliveStrategy()
        {
            IncomingHeartbeatEnabled = true;
            OutgoingHeartbeatEnabled = true;

            ThresholdFailuerCount = 3;
        }

        public override void Setup()
        {
            base.Setup();
        }

        public override void BeforeSessionSetup(LinkSession linkSession)
        {
            linkSession.HeartbeatStrategy = new KeepaliveSessionStrategy {
                Session = linkSession
            };
        }
    }

    public class ClientKeepaliveStrategy : KeepaliveStrategy
    {
        public new AbstractTcpClient Link
        {
            get { return (AbstractTcpClient)base.Link; }
        }

        public override void OnHeartbeat()
        {
            var linkSession = Link.Session;
            if (linkSession == null)
            {
                return;
            }
            var tcpSession = linkSession as AbstractTcpSession;
            if (tcpSession != null && !tcpSession.SocketConnected)
            {
                return;
            }

            var sessionStrategy =
                (KeepaliveSessionStrategy)linkSession.HeartbeatStrategy;

            if (sessionStrategy.OnHeartbeat())
            {
                Trace.Warn("{0} {1} closing due to the keepalive failure",
                    Link.Name, linkSession.Handle);
                
                linkSession.Close();
            }
        }
    }

    public class ServerKeepaliveStrategy : KeepaliveStrategy
    {
        public new AbstractTcpServer Link
        {
            get { return (AbstractTcpServer)base.Link; }
        }

        public override void OnHeartbeat()
        {
            List<LinkSession> snapshot = Link.TakeSessionsSnapshot();
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                var linkSession = snapshot[i];
                var tcpSession = linkSession as AbstractTcpSession;
                if (tcpSession != null && !tcpSession.SocketConnected)
                {
                    continue;
                }

                var sessionStrategy =
                    (KeepaliveSessionStrategy)linkSession.HeartbeatStrategy;

                if (sessionStrategy.OnHeartbeat())
                {
                    Trace.Warn("{0} {1} closing due to the keepalive failure",
                        Link.Name, linkSession.Handle);

                    linkSession.Close();
                }
            }
        }
    }

    public class KeepaliveSessionStrategy : HeartbeatStrategy.SubStrategy
    {
        private int successiveFailureCount;
        private volatile bool hasReceived;
        private volatile bool hasSent;

        public bool HasReceived
        {
            get { return hasReceived; }
            set { hasReceived = value; }
        }
        public bool HasSent
        {
            get { return hasSent; }
            set { hasSent = value; }
        }

        public override bool Process(Event e)
        {
            switch (e.GetTypeId())
            {
                case BuiltinEventType.HeartbeatEvent:
                    // Do nothing
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override bool OnHeartbeat()
        {
            var linkStrategy = (KeepaliveStrategy)Session.Link.HeartbeatStrategy;

            if (linkStrategy.OutgoingHeartbeatEnabled)
            {
                if (hasSent)
                {
                    hasSent = false;
                }
                else
                {
                    Trace.Log("{0} {1} sending heartbeat event",
                        Session.Link.Name, Session.InternalHandle);

                    Session.Send(Hub.HeartbeatEvent);
                }
            }

            if (linkStrategy.IncomingHeartbeatEnabled)
            {
                if (hasReceived)
                {
                    hasReceived = false;
                    Interlocked.Exchange(ref successiveFailureCount, 0);
                }
                else
                {
                    int result = Interlocked.Increment(ref successiveFailureCount);

                    Trace.Info("{0} {1} keepalive failure count {2}",
                        Session.Link.Name, Session.InternalHandle, result);

                    if (!Marked &&
                        result >= linkStrategy.ThresholdFailuerCount)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void OnReceive()
        {
            hasReceived = true;
        }

        public override void OnSend(Event e)
        {
            if (!hasSent &&
                e.GetTypeId() != BuiltinEventType.HeartbeatEvent)
            {
                hasSent = true;
            }
        }
    }
}
