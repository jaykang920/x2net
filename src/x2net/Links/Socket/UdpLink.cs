using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Non-reliable UDP/IP link based on the Begin/End pattern.
    /// </summary>
    public class UdpLink : AbstractUdpLink
    {
        private Segment rxSegment;
        private Segment txSegment;

        /// <summary>
        /// Initializes a new instance of the UdpLink class.
        /// </summary>
        public UdpLink(string name)
            : base(name)
        {
            rxSegment = rxBuffer.FirstSegment;
            txSegment = txBuffer.FirstSegment;
        }

        protected override void ReceiveFromInternal()
        {
            var endPoint = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
            socket.BeginReceiveFrom(
                rxSegment.Array,
                rxSegment.Offset,
                rxBuffer.BlockSize,
                SocketFlags.None,
                ref endPoint,
                OnReceiveFrom,
                null);
        }

        protected override void SendToInternal(EndPoint endPoint)
        {
            socket.BeginSendTo(
                txSegment.Array,
                txSegment.Offset,
                (int)txBuffer.Length,
                SocketFlags.None,
                endPoint,
                OnSendTo,
                null);
        }

        // Asynchronous callback for BeginReceiveFrom
        private void OnReceiveFrom(IAsyncResult asyncResult)
        {
            var endPoint = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
            try
            {
                int bytesTransferred = socket.EndReceiveFrom(asyncResult, ref endPoint);

                if (bytesTransferred > 0)
                {
                    OnReceiveFromInternal(bytesTransferred, endPoint);
                }
                else
                {
                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                var se = e as SocketException;
                if (se != null)
                {
                    if (se.SocketErrorCode == SocketError.OperationAborted)
                    {
                        // Socket has been closed.
                        return;
                    }
                }

                Trace.Warn("{0} recvfrom error {1}", Name, e);
            }

            BeginReceiveFrom();
        }

        // Asynchronous callback for BeginSendTo
        private void OnSendTo(IAsyncResult asyncResult)
        {
            int bytesTransferred = 0;
            try
            {
                bytesTransferred = socket.EndSendTo(asyncResult);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception e)
            {
                var se = e as SocketException;
                if (se != null)
                {
                    if (se.SocketErrorCode == SocketError.OperationAborted)
                    {
                        // Socket has been closed.
                        return;
                    }
                }

                Trace.Warn("{0} sendto error {1}", Name, e);
            }

            OnSendToInternal(bytesTransferred);
        }
    }
}
