using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Non-reliable UDP/IP link based on the SocketAsyncEventArgs pattern.
    /// </summary>
    public class AsyncUdpLink : AbstractUdpLink
    {
        private SocketAsyncEventArgs rxEventArgs;
        private SocketAsyncEventArgs txEventArgs;

        /// <summary>
        /// Initializes a new instance of the AsyncUdpLink class.
        /// </summary>
        public AsyncUdpLink(string name)
            : base(name)
        {
            rxEventArgs = new SocketAsyncEventArgs();
            txEventArgs = new SocketAsyncEventArgs();

            rxEventArgs.Completed += OnReceiveFromCompleted;
            txEventArgs.Completed += OnSendToCompleted;

            Segment segment = rxBuffer.FirstSegment;
            rxEventArgs.SetBuffer(segment.Array, segment.Offset, rxBuffer.BlockSize);
            segment = txBuffer.FirstSegment;
            txEventArgs.SetBuffer(segment.Array, segment.Offset, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            rxEventArgs.Completed -= OnReceiveFromCompleted;
            rxEventArgs.Dispose();

            Trace.Debug("{0} freed rxEventArgs", Name);

            txEventArgs.Completed -= OnSendToCompleted;
            txEventArgs.Dispose();

            Trace.Debug("{0} freed txEventArgs", Name);

            base.Dispose(disposing);
        }

        protected override void ReceiveFromInternal()
        {
            var endPoint = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
            rxEventArgs.RemoteEndPoint = endPoint;

            bool pending = socket.ReceiveFromAsync(rxEventArgs);
            if (!pending)
            {
                Trace.Debug("{0} ReceiveFromAsync completed immediately", Name);

                OnReceiveFrom(rxEventArgs);
            }
        }

        protected override void SendToInternal(EndPoint endPoint)
        {
            txEventArgs.SetBuffer(txEventArgs.Offset, (int)txBuffer.Length);
            txEventArgs.RemoteEndPoint = endPoint;

            bool pending = socket.SendToAsync(txEventArgs);
            if (!pending)
            {
                Trace.Debug("{0} SendToAsync completed immediately", Name);

                OnSendTo(txEventArgs);
            }
        }

        // Completed event handler for ReceiveFromAsync
        private void OnReceiveFromCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnReceiveFrom(e);
        }

        // Completed event handler for SendToAsync
        private void OnSendToCompleted(object sender, SocketAsyncEventArgs e)
        {
            OnSendTo(e);
        }

        // Completion callback for ReceiveFromAsync
        private void OnReceiveFrom(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    OnReceiveFromInternal(e.BytesTransferred, e.RemoteEndPoint);
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (e.SocketError == SocketError.OperationAborted)
                {
                    // Socket has been closed.
                    return;
                }
                else
                {
                    Trace.Warn("{0} recvfrom error {1}", Name, e.SocketError);
                }
            }

            BeginReceiveFrom();
        }

        // Completion callback for SendToAsync
        private void OnSendTo(SocketAsyncEventArgs e)
        {
            e.BufferList = null;

            int bytesTransferred = 0;
            if (e.SocketError == SocketError.Success)
            {
                bytesTransferred = e.BytesTransferred;
            }
            else
            {
                if (e.SocketError == SocketError.OperationAborted)
                {
                    // Socket has been closed.
                    return;
                }
                else
                {
                    Trace.Warn("{0} sendto error {1}", Name, e.SocketError);
                }
            }

            OnSendToInternal(bytesTransferred);
        }
    }
}
