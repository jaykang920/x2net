// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace x2net
{
    /// <summary>
    /// Abstract base class for TCP/IP server links.
    /// </summary>
    public abstract class AbstractTcpServer : ServerLink
    {
        protected Socket socket;

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
                    Name, endpoint, e);

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

            try
            {
                if (!base.OnAcceptInternal(session))
                {
                    return false;
                }

                var clientSocket = tcpSession.Socket;

                // Adjust client socket options.
                clientSocket.NoDelay = NoDelay;

                tcpSession.BeginReceive(true);

                Trace.Info("{0} {1} accepted from {2}",
                    Name, tcpSession.InternalHandle, clientSocket.RemoteEndPoint);

                return true;
            }
            catch (ObjectDisposedException)
            {
                Trace.Log("{0} {1} accept error: closed immediately",
                    Name, tcpSession.InternalHandle);
                return false;
            }
            catch (Exception ex)
            {
                Trace.Warn("{0} {1} accept error: {2}",
                    Name, tcpSession.InternalHandle, ex);
                return false;
            }
        }
    }
}
