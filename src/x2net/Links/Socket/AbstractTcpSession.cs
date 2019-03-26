// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Net;
using System.Net.Sockets;

namespace x2net
{
    /// <summary>
    /// Abstract base class for TCP/IP link sessions.
    /// </summary>
    public abstract class AbstractTcpSession : LinkSession
    {
        protected Socket socket;

        private IPEndPoint remoteEndPoint;

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

        protected internal override int InternalHandle
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
                    Trace.Info("{0} {1} close : {2}",
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

            Trace.Log("{0} {1} built {2}-byte header {3} | {4}",
                link.Name, InternalHandle, sendBuffer.HeaderLength,
                sendBuffer.Buffer.Length, transformed);
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

        protected override void OnEventReceived(Event e)
        {
            TraceLevel traceLevel = 
                (e.GetTypeId() == BuiltinEventType.HeartbeatEvent ?
                TraceLevel.Trace : TraceLevel.Debug);

            Trace.Emit(traceLevel, "{0} {1} received event {2}",
                link.Name, InternalHandle, e);

            base.OnEventReceived(e);
        }

        protected override void OnEventSent(Event e)
        {
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

                Trace.Emit(traceLevel, "{0} {1} sending event {2}",
                    link.Name, InternalHandle, description);
            }

            base.OnEventSent(e);
        }
    }
}
