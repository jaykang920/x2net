// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// TCP/IP link session based on the SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncTcpSession : AbstractTcpSession
    {
        private SocketAsyncEventArgs rxEventArgs;
        private SocketAsyncEventArgs txEventArgs;

        public AsyncTcpSession(SessionBasedLink link, Socket socket)
            : base(link, socket)
        {
            rxEventArgs = new SocketAsyncEventArgs();
            txEventArgs = new SocketAsyncEventArgs();

            rxEventArgs.Completed += OnReceiveCompleted;
            txEventArgs.Completed += OnSendCompleted;
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            rxEventArgs.Completed -= OnReceiveCompleted;
            rxEventArgs.Dispose();

            Log.Debug("{0} {1} freed recvEventArgs", link.Name, handle);

            txEventArgs.Completed -= OnSendCompleted;
            txEventArgs.Dispose();

            Log.Debug("{0} {1} freed sendEventArgs", link.Name, handle);

            base.Dispose(disposing);
        }

        protected override void ReceiveInternal()
        {
            try
            {
                rxBufferList.Clear();
                rxBuffer.ListAvailableSegments(rxBufferList);
                rxEventArgs.BufferList = rxBufferList;

                bool pending = socket.ReceiveAsync(rxEventArgs);
                if (!pending)
                {
                    Log.Debug("{0} {1} ReceiveAsync completed immediately",
                        link.Name, handle);

                    OnReceive(rxEventArgs);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Log.Warn("{0} {1} recv error {2}", link.Name, InternalHandle, e);

                OnDisconnect();
            }
        }

        protected override void SendInternal()
        {
            try
            {
                txEventArgs.BufferList = txBufferList;

                bool pending = socket.SendAsync(txEventArgs);
                if (!pending)
                {
                    Log.Debug("{0} {1} SendAsync completed immediately",
                        link.Name, InternalHandle);

                    OnSend(txEventArgs);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Log.Warn("{0} {1} send error {2}", link.Name, InternalHandle, e);

                OnDisconnect();
            }
        }

        // Completed event handler for ReceiveAsync
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnReceive(e);
        }

        // Completed event handler for SendAsync
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnSend(e);
        }

        // Completion callback for ReceiveAsync
        private void OnReceive(SocketAsyncEventArgs e)
        {
            try
            {
                e.BufferList = null;

                switch (e.SocketError)
                {
                    case SocketError.Success:
                        if (e.BytesTransferred > 0)
                        {
                            OnReceiveInternal(e.BytesTransferred);
                            return;
                        }
                        // (e.BytesTransferred == 0) implies a graceful shutdown
                        Log.Info("{0} {1} disconnected", link.Name, InternalHandle);
                        break;
                    case SocketError.OperationAborted:
                        return;
                    default:
                        Log.Warn("{0} {1} recv error {2}", link.Name, InternalHandle, e.SocketError);
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                Log.Warn("{0} {1} recv error {2}", link.Name, InternalHandle, ex);
            }
            OnDisconnect();
        }

        // Completion callback for SendAsync
        private void OnSend(SocketAsyncEventArgs e)
        {
            try
            {
                e.BufferList = null;

                switch (e.SocketError)
                {
                    case SocketError.Success:
                        OnSendInternal(e.BytesTransferred);
                        return;
                    case SocketError.OperationAborted:
                        return;
                    default:
                        Log.Warn("{0} {1} send error {2}", link.Name, InternalHandle, e.SocketError);
                        break;
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                Log.Warn("{0} {1} send error {2}", link.Name, InternalHandle, ex);
            }
            OnDisconnect();
        }
    }
}
