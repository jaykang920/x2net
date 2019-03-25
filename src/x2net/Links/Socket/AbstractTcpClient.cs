// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Abstract base class for TCP/IP client links.
    /// </summary>
    public abstract class AbstractTcpClient : ClientLink
    {
        private int retryCount;
        private DateTime startTime;
        private EndPoint remoteEndPoint;

        private volatile bool connecting;

        // Connection properties

        /// <summary>
        /// Gets whether this client link is currently connected or not.
        /// </summary>
        public bool Connected
        {
            get
            {
                try
                {
                    using (new ReadLock(rwlock))
                    {
                        return (session != null &&
                            ((AbstractTcpSession)session).SocketConnected);
                    }
                }
                catch
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// Gets or sets the remote host address string to connect to.
        /// </summary>
        public string RemoteHost { get; set; }
        /// <summary>
        /// Gets or sets the remote port number to connect to.
        /// </summary>
        public int RemotePort { get; set; }

        // Socket option properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether the client sockets
        /// are not to use the Nagle algorithm.
        /// </summary>
        public bool NoDelay { get; set; }

        // Connect properties

        /// <summary>
        /// Gets or sets the maximum number of connection retries before this
        /// link declares a connection failure.
        /// </summary>
        /// <remarks>
        /// Default value is 0 (no retry). A negative integer such as -1 means
        /// that the link should retry for unlimited times.
        /// </remarks>
        public int MaxRetryCount { get; set; }
        /// <summary>
        /// Gets or sets the initial connection retry interval time in milliseconds.
        /// </summary>
        public double RetryInterval { get; set; }

        // Reconnect properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether this link should
        /// start a new connection attempt automatically on disconnect, toward
        /// the previous remote endpoint.
        /// </summary>
        public bool AutoReconnect { get; set; }
        /// <summary>
        /// Gets or sets the average delay before automatic reconnect, 
        /// in milliseconds, around which an actual delay is picked randomly.
        /// </summary>
        public int ReconnectDelay { get; set; }

        /// <summary>
        /// Initializes a new instance of the AbstractTcpClient class.
        /// </summary>
        protected AbstractTcpClient(string name)
            : base(name)
        {
            // Default socket options
            NoDelay = true;

            RetryInterval = 1000;  // 1 sec by default

            ReconnectDelay = 1000;  // 1 sec by default
        }

        /// <summary>
        /// Connects to the endpoint RemoteHost:RemotePort.
        /// </summary>
        public void Connect()
        {
            Connect(RemoteHost, RemotePort);
        }

        /// <summary>
        /// Connects to the specified remote address (host:port).
        /// </summary>
        public void Connect(string address)
        {
            Connect(address, 0);
        }

        /// <summary>
        /// Connects to the specified remote address. The port parameter is used
        /// when the given address does not contain a remote port number,
        /// </summary>
        public void Connect(string address, int port)
        {
            int index = address.LastIndexOf(':');
            if (index >= 0 && address.Length > 15)
            {
                // Might be an IPv6 strig without a port specification.
                if (address.Split(':').Length > 6 &&
                    address.LastIndexOf("]:") < 0)
                {
                    index = -1;
                }
            }
            if (0 < index && index < (address.Length - 1))
            {
                string portString = address.Substring(index + 1).Trim();
                if (Int32.TryParse(portString, out port))
                {
                    address = address.Substring(0, index).Trim();
                }
            }

            RemoteHost = address;
            RemotePort = port;

            IPAddress ip;
            try
            {
                ip = Dns.GetHostAddresses(address)[0];
            }
            catch (Exception e)
            {
                Trace.Error("{0} error resolving target host {1} : {2}",
                    Name, address, e.Message);
                throw;
            }

            Connect(ip, RemotePort);
        }

        /// <summary>
        /// Connects to the specified IP address and port.
        /// </summary>
        public void Connect(IPAddress ip, int port)
        {
            connecting = true;

            LinkSession session = Session;
            if (session != null &&
                ((AbstractTcpSession)session).SocketConnected)
            {
                Disconnect();
                Trace.Info("{0} disconnected to initiate a new connection", Name);
            }

            Connect(null, new IPEndPoint(ip, port));
        }

        public void Disconnect()
        {
            LinkSession session;
            using (new WriteLock(rwlock))
            {
                if (ReferenceEquals(this.session, null))
                {
                    return;
                }
                session = this.session;
                this.session = null;
            }
            session.Close();
        }

        private void Connect(Socket socket, EndPoint endpoint)
        {
            Trace.Info("{0} connecting to {1}", Name, endpoint);

            startTime = DateTime.UtcNow;

            ConnectInternal(socket, endpoint);
        }

        /// <summary>
        /// <see cref="ClientLink.Reconnect"/>
        /// </summary>
        public override void Reconnect()
        {
            if (remoteEndPoint == null)
            {
                Trace.Error("{0} no reconnect target", Name);
                return;
            }

            if (connecting)
            {
                return;
            }

            Connect(null, remoteEndPoint);
        }

        protected override void OnSessionConnectedInternal(bool result, object context)
        {
            base.OnSessionConnectedInternal(result, context);

            connecting = false;
        }

        protected override void OnSessionDisconnectedInternal(int handle, object context)
        {
            base.OnSessionDisconnectedInternal(handle, context);

            if (!closing)
            {
                if (AutoReconnect)
                {
                    if (ReconnectDelay > 0)
                    {
                        int max = (ReconnectDelay << 1) + 1;
                        int delay = new Random().Next(max);

                        Thread.Sleep(delay);
                    }

                    Reconnect();
                }
            }
        }

        /// <summary>
        /// Provides an actual implementation of asynchronous Connect.
        /// </summary>
        protected abstract void ConnectInternal(Socket socket, EndPoint endpoint);

        /// <summary>
        /// <see cref="ClientLink.OnConnectInternal"/>
        /// </summary>
        protected override void OnConnectInternal(LinkSession session)
        {
            base.OnConnectInternal(session);

            // Reset the retry counter.
            retryCount = 0;

            var tcpSession = (AbstractTcpSession)session;
            Socket socket = tcpSession.Socket;

            // Adjust socket options.
            socket.NoDelay = NoDelay;

            // Save the remote endpoint to reconnect.
            remoteEndPoint = socket.RemoteEndPoint;

            tcpSession.BeginReceive(true);

            Trace.Info("{0} {1} connected to {2}",
                Name, tcpSession.InternalHandle, socket.RemoteEndPoint);
        }

        /// <summary>
        /// Called by a derived link class when a connection attempt fails.
        /// </summary>
        protected virtual void OnConnectError(Socket socket, EndPoint endpoint)
        {
            if (MaxRetryCount < 0 ||
                (MaxRetryCount > 0 && retryCount < MaxRetryCount))
            {
                // Exponential backoff applied (up to x8)
                int count = Math.Min(retryCount, 3);
                double interval = RetryInterval * (1 << count);

                ++retryCount;

                double elapsedMillisecs =
                    (DateTime.UtcNow - startTime).TotalMilliseconds;
                if (elapsedMillisecs < interval)
                {
                    Thread.Sleep((int)(interval - elapsedMillisecs));
                }

                Connect(socket, endpoint);
            }
            else
            {
                socket.Close();

                connecting = false;

                OnLinkSessionConnectedInternal(false, endpoint);
            }
        }

        protected override void SetupInternal()
        {
            base.SetupInternal();

            var holdingFlow = Flow;
            if (!ReferenceEquals(holdingFlow, null))
            {
                holdingFlow.SubscribeTo(Name);
            }
        }

        protected override void TeardownInternal()
        {
            var holdingFlow = Flow;
            if (!ReferenceEquals(holdingFlow, null))
            {
                holdingFlow.UnsubscribeFrom(Name);
            }

            base.TeardownInternal();
        }
    }
}
