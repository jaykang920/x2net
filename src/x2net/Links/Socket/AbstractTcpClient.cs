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
        private class PendingRecord
        {
            public bool HandleAllocated { get; set; }
            public int Count { get; set; }
            public Timer.Token TimeoutToken { get; set; }
        }

        private int retryCount;
        private DateTime startTime;
        private EndPoint remoteEndPoint;

        private volatile bool connecting;

        private List<Event> eventQueue;
        private bool connected;
        private int sessionRefCount;
        private Dictionary<int, PendingRecord> pendingRecords;

        // Connection properties

        /// <summary>
        /// Gets whether this client link is currently connected or not.
        /// </summary>
        public bool Connected
        {
            get
            {
                using (new ReadLock(rwlock))
                {
                    return (session != null &&
                        ((AbstractTcpSession)session).SocketConnected);
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


        // Connect-on-Demand properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether the connection is to
        /// be closed on completion of requested tasks.
        /// </summary>
        public bool DisconnectOnComplete { get; set; }
        /// <summary>
        /// Gets or sets a length of time, in milliseconds, after which a
        /// disconnection is considered when the DisconnectOnComplete is set.
        /// </summary>
        public int DisconnectDelay { get; set; }
        public int ResponseTimeout { get; set; }

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

            DisconnectDelay = 200;  // 200ms by default
            ResponseTimeout = 30;  // 30 seconds by default

            eventQueue = new List<Event>();
            pendingRecords = new Dictionary<int, PendingRecord>();
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

        public void ConnectAndSend(Event e)
        {
            using (new UpgradeableReadLock(rwlock))
            {
                if (connected)
                {
                    session.Send(e);
                    return;
                }
                else
                {
                    using (new WriteLock(rwlock))
                    {
                        // double check
                        if (connected)
                        {
                            session.Send(e);
                            return;
                        }
                        else
                        {
                            eventQueue.Add(e);
                            if (connecting)
                            {
                                return;
                            }
                            else
                            {
                                connecting = true;
                            }
                        }
                    }
                }
            }

            Connect();
        }

        public void ConnectAndRequest(Event req)
        {
            int waitHandle = req._WaitHandle;

            bool handleAllocated = false;
            if (waitHandle == 0)
            {
                waitHandle = WaitHandlePool.Acquire();
                req._WaitHandle = waitHandle;
                handleAllocated = true;
            }

            lock (pendingRecords)
            {
                PendingRecord pendingRecord;
                if (!pendingRecords.TryGetValue(waitHandle, out pendingRecord))
                {
                    TimeoutEvent timeoutEvent = new TimeoutEvent { Key = waitHandle };

                    Bind(new Event { _WaitHandle = waitHandle }, OnEvent);
                    Bind(timeoutEvent, OnTimeout);

                    pendingRecord = new PendingRecord {
                        HandleAllocated = handleAllocated,
                        TimeoutToken = 
                            TimeFlow.Instance.Reserve(timeoutEvent, ResponseTimeout)
                    };
                    pendingRecords.Add(waitHandle, pendingRecord);
                }
                ++pendingRecord.Count;
            }

            Interlocked.Increment(ref sessionRefCount);
            
            ConnectAndSend(req);
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
                connected = false;
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

            if (result)
            {
                using (new WriteLock(rwlock))
                {
                    connected = true;

                    for (int i = 0, count = eventQueue.Count; i < count; ++i)
                    {
                        ((LinkSession)context).Send(eventQueue[i]);
                    }
                    eventQueue.Clear();

                    if (DisconnectOnComplete)
                    {
                        TimeFlow.Instance.ReserveRepetition(new TimeoutEvent {
                            _Channel = Name,
                            Key = this
                        }, new TimeSpan(0, 0, 0, 0, DisconnectDelay));
                    }
                }
            }
        }

        protected override void OnSessionDisconnectedInternal(int handle, object context)
        {
            base.OnSessionDisconnectedInternal(handle, context);

            using (new WriteLock(rwlock))
            {
                connected = false;

                if (DisconnectOnComplete)
                {
                    TimeFlow.Instance.CancelRepetition(new TimeoutEvent {
                        _Channel = Name,
                        Key = this
                    });
                }
            }

            if (!closing)
            {
                if (AutoReconnect && !DisconnectOnComplete)
                {
                    if (ReconnectDelay > 0)
                    {
                        int min = ReconnectDelay >> 1;
                        int max = ReconnectDelay + min + 1;
                        int delay = new Random().Next(min, max);

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

            Bind(new TimeoutEvent { Key = this }, OnTimer);
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

        private void OnTimer(TimeoutEvent e)
        {
            if (!ReferenceEquals(session, null))
            {
                if (!session.IsBusy && sessionRefCount == 0)
                {
                    Disconnect();
                }
            }
        }

        private void OnEvent(Event e)
        {
            int waitHandle = e._WaitHandle;

            PendingRecord pendingRecord;
            lock (pendingRecords)
            {
                if (!pendingRecords.TryGetValue(waitHandle, out pendingRecord))
                {
                    return;
                }
                if (--pendingRecord.Count == 0)
                {
                    pendingRecords.Remove(waitHandle);
                }
            }

            if (pendingRecord.Count == 0)
            {
                if (pendingRecord.HandleAllocated)
                {
                    WaitHandlePool.Release(waitHandle);
                }

                TimeFlow.Instance.Cancel(pendingRecord.TimeoutToken);
                Unbind(new Event { _WaitHandle = waitHandle }, OnEvent);
                Unbind((TimeoutEvent)pendingRecord.TimeoutToken.value, OnTimeout);
            }

            Interlocked.Decrement(ref sessionRefCount);
        }

        private void OnTimeout(TimeoutEvent e)
        {
            Trace.Debug("{0} response timeout {1}", Name, e.Key);

            int waitHandle = (int)e.Key;

            int count = 0;
            PendingRecord pendingRecord;
            lock (pendingRecords)
            {
                if (!pendingRecords.TryGetValue(waitHandle, out pendingRecord))
                {
                    return;
                }
                count = pendingRecord.Count;
                pendingRecords.Remove(waitHandle);
            }

            if (pendingRecord.HandleAllocated)
            {
                WaitHandlePool.Release(waitHandle);
            }

            Unbind(new Event { _WaitHandle = waitHandle }, OnEvent);
            Unbind((TimeoutEvent)pendingRecord.TimeoutToken.value, OnTimeout);

            Interlocked.Add(ref sessionRefCount, -count);
        }
    }
}
