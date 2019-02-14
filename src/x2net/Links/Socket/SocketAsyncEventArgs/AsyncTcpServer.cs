// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Net.Sockets;

namespace x2net
{
    /// <summary>
    /// TCP/IP server link based on the SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpServer : AbstractTcpServer
    {
        private const int numConcurrentAcceptors = 16;

        private SocketAsyncEventArgs[] acceptEventArgs;

        /// <summary>
        /// Initializes a new instance of the AsyncTcpServer class.
        /// </summary>
        public AsyncTcpServer(string name)
            : base(name)
        {
            acceptEventArgs = new SocketAsyncEventArgs[numConcurrentAcceptors];

            for (int i = 0; i < numConcurrentAcceptors; ++i)
            {
                var saea = new SocketAsyncEventArgs();
                saea.Completed += OnAcceptCompleted;
                acceptEventArgs[i] = saea;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            for (int i = 0; i < numConcurrentAcceptors; ++i)
            {
                var saea = acceptEventArgs[i];
                saea.Completed -= OnAcceptCompleted;
                saea.Dispose();
            }

            acceptEventArgs = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// <see cref="AbstractTcpServer.AcceptInternal"/>
        /// </summary>
        protected override void AcceptInternal()
        {
            for (int i = 0, count = acceptEventArgs.Length; i < count; ++i)
            {
                AcceptImpl(acceptEventArgs[i]);
            }
        }

        private void AcceptImpl(SocketAsyncEventArgs e)
        {
            e.AcceptSocket = null;

            bool pending = socket.AcceptAsync(e);
            if (!pending)
            {
                OnAccept(e);
            }
        }

        // Completed event handler for AcceptAsync
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnAccept(e);
        }

        // Completion callback for AcceptAsync
        private void OnAccept(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    var clientSocket = e.AcceptSocket;
                    var session = new AsyncTcpSession(this, clientSocket);

                    if (!OnAcceptInternal(session))
                    {
                        OnLinkSessionConnectedInternal(false, clientSocket.RemoteEndPoint);
                        session.CloseInternal();
                    }
                }
                else
                {
                    if (e.SocketError == SocketError.OperationAborted)
                    {
                        Trace.Info("{0} listening socket closed", Name);
                        return;
                    }
                    else
                    {
                        Trace.Info("{0} accept error : {1}", Name, e.SocketError);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // log
            }
            catch (Exception ex)
            {
                Trace.Info("{0} accept error : {1}", Name, ex);
            }

            AcceptImpl(e);  // chain into the next accept
        }
    }
}
