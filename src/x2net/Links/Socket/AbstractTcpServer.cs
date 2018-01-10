// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Abstract base class for TCP/IP server links.
    /// </summary>
    public abstract class AbstractTcpServer : ServerLink
    {
        protected Socket socket;

        private volatile bool incomingKeepaliveEnabled;
        private volatile bool outgoingKeepaliveEnabled;

        /// <summary>
        /// Gets whether this link is currently listening or not.
        /// </summary>
        public bool Listening
        {
            get { return (socket != null && socket.IsBound); }
        }

        // Socket option properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether the client sockets
        /// are not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        // Keepalive properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether this link checks
        /// for incomming keepalive events
        /// </summary>
        public bool IncomingKeepaliveEnabled
        {
            get { return incomingKeepaliveEnabled; }
            set
            {
                incomingKeepaliveEnabled = value;
            }
        }
        /// <summary>
        /// Gets or sets a boolean value indicating whether this link emits
        /// outgoing keepalive events.
        /// </summary>
        public bool OutgoingKeepaliveEnabled
        {
            get { return outgoingKeepaliveEnabled; }
            set
            {
                outgoingKeepaliveEnabled = value;
            }
        }
        /// <summary>
        /// Gets or sets the maximum number of successive keepalive failures
        /// allowed before the link closes the session.
        /// </summary>
        public int MaxKeepaliveFailureCount { get; set; }
        /// <summary>
        /// Gets or sets a boolean value indicating whether this link ignores
        /// keepalive failures.
        /// </summary>
        public bool IgnoreKeepaliveFailure { get; set; }

        static AbstractTcpServer()
        {
            EventFactory.Global.Register(HeartbeatEvent.TypeId, HeartbeatEvent.New);
        }

        /// <summary>
        /// Initializes a new instance of the AbstractTcpServer class.
        /// </summary>
        protected AbstractTcpServer(string name) : base(name)
        {
            // Default socket options
            NoDelay = true;
        }

        public void Listen(int port)
        {
            Listen(IPAddress.Any, port);
        }

        public void Listen(string ip, int port)
        {
            Listen(IPAddress.Parse(ip), port);
        }

        public void Listen(IPAddress ip, int port)
        {
            if (socket != null)
            {
                throw new InvalidOperationException();
            }

            EndPoint endpoint = new IPEndPoint(ip, port);
            try
            {
                socket = new Socket(ip.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endpoint);
                socket.Listen(Int32.MaxValue);

                AcceptInternal();

                Trace.Info("{0} listening on {1}", Name, endpoint);
            }
            catch (Exception e)
            {
                Trace.Error("{0} error listening on {1} : {2}",
                    Name, endpoint, e.Message);

                throw;
            }
        }

        /// <summary>
        /// Provides an actual implementation of Accept.
        /// </summary>
        protected abstract void AcceptInternal();

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (socket != null)
            {
                socket.Close();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// <see cref="ServerLink.OnAcceptInternal"/>
        /// </summary>
        protected override bool OnAcceptInternal(LinkSession session)
        {
            var tcpSession = (AbstractTcpSession)session;
            var clientSocket = tcpSession.Socket;

            try
            {
                // Adjust client socket options.
                clientSocket.NoDelay = NoDelay;

                tcpSession.BeginReceive(true);

                Trace.Info("{0} {1} accepted from {2}",
                    Name, tcpSession.InternalHandle, clientSocket.RemoteEndPoint);

                return base.OnAcceptInternal(session);

            }
            catch (ObjectDisposedException ode)
            {
                Trace.Warn("{0} {1} accept error: {2}",
                    Name, tcpSession.InternalHandle, ode);
                return false;
            }
        }

        protected override void SetupInternal()
        {
            base.SetupInternal();

            Bind(Hub.HeartbeatEvent, OnHeartbeatEvent);
        }

        private void OnHeartbeatEvent(HeartbeatEvent e)
        {
            if (!IncomingKeepaliveEnabled && !OutgoingKeepaliveEnabled)
            {
                return;
            }

            List<LinkSession> snapshot;
            using (new ReadLock(rwlock))
            {
                snapshot = new List<LinkSession>(sessions.Count);
                var list = sessions.Values;
                for (int i = 0, count = list.Count; i < count; ++i)
                {
                    snapshot.Add(list[i]);
                }
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                var tcpSession = (AbstractTcpSession)snapshot[i];
                if (tcpSession.SocketConnected)
                {
                    int failureCount = tcpSession.Keepalive(
                        incomingKeepaliveEnabled, outgoingKeepaliveEnabled);

                    if (MaxKeepaliveFailureCount > 0 &&
                        failureCount > MaxKeepaliveFailureCount)
                    {
                        Trace.Warn("{0} {1} closed due to the keepalive failure",
                            Name, tcpSession.Handle);

                        tcpSession.OnDisconnect();
                    }
                }
            }
        }
    }
}
