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
    /// Abstract base class for TCP/IP link sessions.
    /// </summary>
    public abstract class AbstractTcpSession : LinkSession
    {
        protected Socket socket;

        private IPEndPoint remoteEndPoint;

        private int keepaliveFailureCount;
        private volatile bool hasReceived;
        private volatile bool hasSent;

        /// <summary>
        /// Gets whether this session is currently connected or not.
        /// </summary>
        public bool SocketConnected
        {
            get { return (socket != null && socket.Connected); }
        }

        /// <summary>
        /// Gets the cached remote endpoint of this session, or null.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get { return remoteEndPoint; } }

        /// <summary>
        /// Gets the underlying Socket object.
        /// </summary>
        public Socket Socket { get { return socket; } }

        // Keepalive properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether this link session
        /// ignores keepalive failures.
        /// </summary>
        public bool IgnoreKeepaliveFailure { get; set; }

        internal bool HasReceived
        {
            get { return hasReceived; }
        }

        internal bool HasSent
        {
            get { return hasSent; }
        }

        internal override int InternalHandle
        {
            get
            {
                if (handle == 0)
                {
                    return -(socket.Handle.ToInt32());
                }
                return base.InternalHandle;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AbstractTcpSession class.
        /// </summary>
        protected AbstractTcpSession(SessionBasedLink link, Socket socket)
            : base(link)
        {
            this.socket = socket;
            remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
        }

        /// <summary>
        /// Called on send/receive error.
        /// </summary>
        public void OnDisconnect()
        {
            if (disposed)
            {
                return;
            }

            CloseInternal();

            OnDisconnect(this);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return String.Format("{0} {1} {2}",
                GetType().Name, InternalHandle, remoteEndPoint);
        }

        protected override void OnClose()
        {
            OnDisconnect(this);
        }

        internal int Keepalive(bool checkIncoming, bool checkOutgoing)
        {
            int result = 0;

            if (checkIncoming)
            {
                if (hasReceived)
                {
                    hasReceived = false;
                    Interlocked.Exchange(ref keepaliveFailureCount, 0);
                }
                else
                {
                    if (!IgnoreKeepaliveFailure)
                    {
                        result = Interlocked.Increment(ref keepaliveFailureCount);

                        if (result > 1)
                        {
                            Trace.Info("{0} {1} keepalive failure count {2}",
                                link.Name, InternalHandle, result);
                        }
                    }
                }
            }

            if (checkOutgoing)
            {
                if (hasSent)
                {
                    hasSent = false;
                }
                else
                {
                    Trace.Log("{0} {1} sent keepalive event",
                        link.Name, InternalHandle);

                    Send(Hub.HeartbeatEvent);
                }
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (socket != null)
            {
                try
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    socket.Close();
                }
                catch (Exception e)
                {
                    Trace.Warn("{0} {1} close : {2}",
                        Link.Name, InternalHandle, e.Message);
                }
            }

            base.Dispose(disposing);
        }

        protected override void BuildHeader(SendBuffer sendBuffer, bool transformed)
        {
            uint header = (uint)(transformed ? 1 : 0);
            header |= ((uint)sendBuffer.Buffer.Length << 1);

            sendBuffer.HeaderLength = Serializer.WriteVariable(sendBuffer.HeaderBytes, header);
        }

        protected override bool ParseHeader()
        {
            uint header;
            int headerLength;
            try
            {
                headerLength = rxBuffer.ReadVariable(out header);
            }
            catch (System.IO.EndOfStreamException)
            {
                // Need more to start parsing.
                rxBuffer.Rewind();  // restore pos to start again
                return false;
            }
            rxBuffer.Shrink(headerLength);
            lengthToReceive = (int)(header >> 1);
            rxTransformed = ((header & 1) != 0);
            return true;
        }

        protected override bool Process(Event e)
        {
            switch (e.GetTypeId())
            {
                case BuiltinEventType.HeartbeatEvent:
                    // Do nothing
                    break;
                default:
                    return base.Process(e);
            }
            return true;
        }

        protected override void OnEventReceived(Event e)
        {
            hasReceived = true;

            TraceLevel traceLevel = 
                (e.GetTypeId() == BuiltinEventType.HeartbeatEvent ?
                TraceLevel.Trace : TraceLevel.Debug);

            Trace.Emit(traceLevel, "{0} {1} received event {2}",
                link.Name, InternalHandle, e);

            base.OnEventReceived(e);
        }

        protected override void OnEventSent(Event e)
        {
            hasSent = true;

            TraceLevel traceLevel =
                (e.GetTypeId() == BuiltinEventType.HeartbeatEvent ?
                TraceLevel.Trace : TraceLevel.Debug);

            if (Trace.Handler != null && Config.TraceLevel <= traceLevel)
            {
                // e.ToString() may crash if a composite property (list for example)
                // of the event is changed in other threads.
                string description;
                try
                {
                    description = e.ToString();
                }
                catch
                {
                    description = e.GetTypeTag().RuntimeType.Name;
                }

                Trace.Emit(traceLevel, "{0} {1} sent event {2}",
                    link.Name, InternalHandle, description);
            }

            base.OnEventSent(e);
        }
    }
}
