// Copyright (c) 2017-2019 Jae-jun Kang
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
        private class ConnectContext
        {
            public Socket Socket { get; set; }
            public EndPoint RemoteEndPoint { get; set; }
        }
        private ConnectContext context;

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
                if (ReferenceEquals(socket, null))
                {
                    socket = new Socket(
                        endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                }

                var context = new ConnectContext {
                    Socket = socket,
                    RemoteEndPoint = endpoint,
                };
                socket.BeginConnect(endpoint, OnConnect, context);
                this.context = context;
            }
            catch (Exception e)
            {
                Trace.Info("{0} error connecting to {1} : {2}",
                    Name, endpoint, e);

                OnConnectError(socket, endpoint);
            }
        }

        // Asynchronous callback for BeginConnect
        private void OnConnect(IAsyncResult asyncResult)
        {
            if (disposed) { return; }

            var context = (ConnectContext)asyncResult.AsyncState;
            var socket = context.Socket;
            try
            {
                socket.EndConnect(asyncResult);

                if (ReferenceEquals(this.context, context))
                {
                    OnConnectInternal(new TcpSession(this, socket));
                    this.context = null;
                }
                else
                {
                    socket.Close();
                }
            }
            catch (Exception e)
            {
                if (ReferenceEquals(this.context, context))
                {
                    Trace.Info("{0} error connecting to {1} : {2}",
                        Name, context.RemoteEndPoint, e);

                    OnConnectError(socket, context.RemoteEndPoint);
                    this.context = null;
                }
            }
        }
    }
}
