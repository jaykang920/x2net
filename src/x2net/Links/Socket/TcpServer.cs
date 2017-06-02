// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// TCP/IP server link based on the Begin/End pattern.
    /// </summary>
    public class TcpServer : AbstractTcpServer
    {
        /// <summary>
        /// Initializes a new instance of the TcpServer class.
        /// </summary>
        public TcpServer(string name)
            : base(name)
        {
        }

        /// <summary>
        /// <see cref="AbstractTcpServer.AcceptInternal"/>
        /// </summary>
        protected override void AcceptInternal()
        {
            socket.BeginAccept(OnAccept, null);
        }

        // Asynchronous callback for BeginAccept
        private void OnAccept(IAsyncResult asyncResult)
        {
            try
            {
                var clientSocket = socket.EndAccept(asyncResult);
                var session = new TcpSession(this, clientSocket);

                if (!OnAcceptInternal(session))
                {
                    OnLinkSessionConnectedInternal(false, clientSocket.RemoteEndPoint);
                    session.CloseInternal();
                }
            }
            catch (ObjectDisposedException)
            {
                Log.Info("{0} listening socket closed", Name);
                return;
            }
            catch (Exception e)
            {
                Log.Error("{0} accept error : {1}", Name, e.Message);
            }

            AcceptInternal();
        }
    }
}
