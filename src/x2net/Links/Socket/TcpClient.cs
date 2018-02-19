// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Net;
using System.Net.Sockets;

namespace x2net
{
    /// <summary>
    /// TCP/IP client link based on the Begin/End pattern.
    /// </summary>
    public class TcpClient : AbstractTcpClient
    {
        private EndPoint remoteEndPoint;

        /// <summary>
        /// Initializes a new instance of the TcpClient class.
        /// </summary>
        public TcpClient(string name)
            : base(name)
        {
        }

        /// <summary>
        /// <see cref="AbstractTcpClient.ConnectInternal"/>
        /// </summary>
        protected override void ConnectInternal(Socket socket, EndPoint endpoint)
        {
            try
            {
                if (Object.ReferenceEquals(socket, null))
                {
                    socket = new Socket(
                        endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                }

                remoteEndPoint = endpoint;
                socket.BeginConnect(endpoint, OnConnect, socket);
            }
            catch (Exception e)
            {
                Trace.Error("{0} error connecting to {1} : {2}",
                    Name, endpoint, e.Message);

                OnConnectError(socket, endpoint);
            }
        }

        // Asynchronous callback for BeginConnect
        private void OnConnect(IAsyncResult asyncResult)
        {
            if (disposed) { return; }

            var socket = (Socket)asyncResult.AsyncState;
            try
            {
                socket.EndConnect(asyncResult);

                OnConnectInternal(new TcpSession(this, socket));
            }
            catch (Exception e)
            {
                Trace.Warn("{0} error connecting to {1} : {2}",
                    Name, remoteEndPoint, e.Message);

                OnConnectError(socket, remoteEndPoint);
            }
        }
    }
}
