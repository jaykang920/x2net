// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Abstract base class for concrete link sessions.
    /// </summary>
    public abstract partial class LinkSession : IDisposable
    {
        protected int handle;
        protected SessionBasedLink link;
        protected bool polarity;

        protected Buffer rxBuffer;
        protected List<SendBuffer> txBuffers;
        protected List<ArraySegment<byte>> rxBufferList;
        protected List<ArraySegment<byte>> txBufferList;

        protected List<Event> eventsSending;
        protected List<Event> eventsToSend;

        protected int lengthToReceive;
        protected int lengthToSend;

        protected bool rxBeginning;
        protected bool rxTransformed;

        protected bool txFlag;

        protected object syncRoot = new Object();

        protected volatile bool connected;
        protected volatile bool disposed;

        /// <summary>
        /// Gets or sets the context object associated with this link session.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Gets or sets the link session handle that is unique in the current
        /// process.
        /// </summary>
        public int Handle
        {
            get { return handle; }
            set { handle = value; }
        }

        public bool IsBusy
        {
            get
            {
                lock (syncRoot)
                {
                    return (txFlag || eventsToSend.Count != 0);
                }
            }
        }

        /// <summary>
        /// Gets the link associated with this session.
        /// </summary>
        public SessionBasedLink Link { get { return link; } }

        /// <summary>
        /// Gets or sets a boolean value indicating whether this session is an
        /// active (client) session. A passive (server) session will return false.
        /// </summary>
        public bool Polarity
        {
            get { return polarity; }
            set { polarity = value; }
        }

        /// <summary>
        /// Gets or sets whether this link session is in connected state or not.
        /// </summary>
        public bool Connected {
            get { return connected; }
            set { connected = value; }
        }

        protected internal virtual int InternalHandle { get { return handle; } }

        internal object SyncRoot { get { return syncRoot; } }

        // Link session strategies

        public volatile ChannelStrategy.SubStrategy ChannelStrategy;
        public volatile HeartbeatStrategy.SubStrategy HeartbeatStrategy;

        public bool HasChannelStrategy
        {
            get { return !ReferenceEquals(ChannelStrategy, null); }
        }
        public bool HasHeartbeatStrategy
        {
            get { return !ReferenceEquals(HeartbeatStrategy, null); }
        }

        /// <summary>
        /// Initializes a new instance of the LinkSession class.
        /// </summary>
        protected LinkSession(SessionBasedLink link)
        {
            this.link = link;

            rxBuffer = new Buffer();
            txBuffers = new List<SendBuffer>();
            rxBufferList = new List<ArraySegment<byte>>();
            txBufferList = new List<ArraySegment<byte>>();

            eventsSending = new List<Event>();
            eventsToSend = new List<Event>();

            Diag = new Diagnostics(this);
        }

        ~LinkSession()
        {
            Dispose(false);
        }

        /// <summary>
        /// Actively closes this link session and releases all the associated
        /// resources.
        /// </summary>
        public virtual void Close()
        {
            OnClose();

            CloseInternal();
        }

        protected abstract void OnClose();

        public void CloseInternal()
        {
            Dispose();
        }

        /// <summary>
        /// Implements IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            lock (syncRoot)
            {
                Dispose(true);
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) { return; }

            disposed = true;
            connected = false;

            Trace.Info("{0} closed {1}", link.Name, this);

            if (HasChannelStrategy)
            {
                ChannelStrategy.Release();
            }

            rxBuffer.Dispose();

            for (int i = 0, count = txBuffers.Count; i < count; ++i)
            {
                txBuffers[i].Dispose();
            }
            txBuffers.Clear();

            txBufferList.Clear();
            rxBufferList.Clear();
        }

        /// <summary>
        /// Sends out the specified event through this link session.
        /// </summary>
        public void Send(Event e)
        {
            lock (syncRoot)
            {
                if (disposed)
                {
                    Trace.Info("{0} {1} dropped {2}", link.Name, InternalHandle, e);
                    return;
                }

                eventsToSend.Add(e);

                if (txFlag)
                {
                    Trace.Debug("{0} {1} buffered {2}", link.Name, InternalHandle, e);
                    return;
                }

                txFlag = true;
            }

            BeginSend();
        }

        internal void BeginReceive(bool beginning)
        {
            lock (syncRoot)
            {
                rxBeginning = beginning;
            }

            ReceiveInternal();
        }

        internal void BeginSend()
        {
            lock (syncRoot)
            {
                if (eventsToSend.Count == 0)
                {
                    return;
                }
                // Swap the event buffers.
                if (eventsSending.Count != 0)
                {
                    eventsSending.Clear();
                }
                List<Event> temp = eventsSending;
                eventsSending = eventsToSend;
                eventsToSend = temp;
                temp = null;
            }

            // Capture send buffers.
            txBufferList.Clear();
            lengthToSend = 0;
            int count = eventsSending.Count;
            int bufferCount = txBuffers.Count;
            if (bufferCount < count)
            {
                for (int i = 0, n = count - bufferCount; i < n; ++i)
                {
                    txBuffers.Add(new SendBuffer());
                }
            }
            else
            {
                for (int i = 0, n = bufferCount - count; i < n; ++i)
                {
                    int j = bufferCount - (i + 1);
                    txBuffers[j].Dispose();
                    txBuffers.RemoveAt(j);
                }
            }
            for (int i = 0; i < count; ++i)
            {
                Event e = eventsSending[i];

                var sendBuffer = txBuffers[i];
                sendBuffer.Reset();

                int typeId = e.GetTypeId();

                Trace.Log("{0} {1} buffering event type {2} to send (#{3}/{4})",
                    link.Name, InternalHandle, typeId, i + 1, count);

                Serializer serializer = new Serializer(sendBuffer.Buffer);
                serializer.Write(typeId);
                e.Serialize(serializer);

                bool transformed = false;

                if (HasChannelStrategy && e._Transform)
                {
                    transformed = ChannelStrategy.BeforeSend(sendBuffer.Buffer, (int)sendBuffer.Buffer.Length);
                }

                BuildHeader(sendBuffer, transformed);

                sendBuffer.ListOccupiedSegments(txBufferList);
                lengthToSend += sendBuffer.Length;

                OnEventSent(e);
            }

            Trace.Log("{0} {1} serialized {2} events to send ({3} bytes)",
                link.Name, InternalHandle, count, lengthToSend);

            SendInternal();
        }

        protected abstract void BuildHeader(SendBuffer sendBuffer, bool transformed);
        protected abstract bool ParseHeader();

        protected abstract void ReceiveInternal();
        internal protected abstract void SendInternal();

        protected void OnReceiveInternal(int bytesTransferred)
        {
            Diag.AddBytesReceived(bytesTransferred);

            if (HasHeartbeatStrategy)
            {
                HeartbeatStrategy.OnReceive();
            }

            lock (syncRoot)
            {
                if (disposed)
                {
                    return;
                }
                rxBuffer.Stretch(bytesTransferred);
            }

            if (Config.TraceLevel <= TraceLevel.Trace)
            {
                Trace.Log("{0} {1} recv {2}: {3}", link.Name, InternalHandle,
                    bytesTransferred, rxBuffer.ToHexString());
            }

            if (rxBeginning)
            {
                rxBuffer.Rewind();

                if (!ParseHeader())
                {
                    BeginReceive(true);
                    return;
                }
            }

            // Handle split packets.
            if (rxBuffer.Length < lengthToReceive)
            {
                BeginReceive(false);
                return;
            }

            while (true)
            {
                rxBuffer.MarkToRead(lengthToReceive);

                Trace.Log("{0} {1} marked {2} byte(s) to read",
                    link.Name, InternalHandle, lengthToReceive);

                if (HasChannelStrategy && rxTransformed)
                {
                    try
                    {
                        ChannelStrategy.AfterReceive(rxBuffer, lengthToReceive);
                    }
                    catch (Exception e)
                    {
                        Trace.Warn("{0} {1} buffer inv transform error: {2}",
                            link.Name, InternalHandle, e);
                        goto next;
                    }
                }
                rxBuffer.Rewind();

                var deserializer = new Deserializer(rxBuffer, link.EventFactory);
                int typeId;
                try
                {
                    deserializer.Read(out typeId);
                }
                catch (System.IO.EndOfStreamException)
                {
                    // Need more
                    goto next;
                }
                Event retrieved = link.CreateEvent(typeId);
                if (ReferenceEquals(retrieved, null))
                {
                    Trace.Info("{0} {1} unknown event type id {2}",
                        link.Name, InternalHandle, typeId);
                    goto next;
                }
                else
                {
                    try
                    {
                        retrieved.Deserialize(deserializer);
                    }
                    catch (Exception e)
                    {
                        Trace.Info("{0} {1} error loading event {2}: {3}",
                            link.Name, InternalHandle, retrieved.GetTypeId(), e.ToString());
                        goto next;
                    }

                    OnEventReceived(retrieved);

                    // Consider subscribing/unsubscribing here.
                    bool processed = false;
                    if (HasChannelStrategy)
                    {
                        processed = ChannelStrategy.Process(retrieved);
                    }
                    if (!processed && HasHeartbeatStrategy)
                    {
                        processed = HeartbeatStrategy.Process(retrieved);
                    }
                    if (!processed)
                    {
                        processed = Process(retrieved);
                    }
                    if (!processed)
                    {
                        retrieved._Handle = Handle;

                        link.OnPreprocess(this, retrieved);

                        Hub.Post(retrieved);
                    }
                }
            next:
                if (disposed)
                {
                    return;
                }

                rxBuffer.Trim();
                if (rxBuffer.IsEmpty || !ParseHeader())
                {
                    break;
                }

                if (rxBuffer.Length < lengthToReceive)
                {
                    BeginReceive(false);
                    return;
                }
            }

            BeginReceive(true);
        }

        protected void OnDisconnect(object context)
        {
            if (handle <= 0)
            {
                return;
            }

            link.OnLinkSessionDisconnectedInternal(handle, context);
        }

        internal protected void OnSendInternal(int bytesTransferred)
        {
            Diag.AddBytesSent(bytesTransferred);

            lock (syncRoot)
            {
                if (disposed)
                {
                    txFlag = false;
                    return;
                }
            }

            if (Config.TraceLevel <= TraceLevel.Trace)
            {
                for (int i = 0; i < txBuffers.Count; ++i)
                {
                    SendBuffer sendBuffer = txBuffers[i];

                    Trace.Log("{0} {1} sent head {2}: {3}", link.Name,
                        InternalHandle, sendBuffer.HeaderLength,
                        BitConverter.ToString(sendBuffer.HeaderBytes, 0, sendBuffer.HeaderLength));
                    Trace.Log("{0} {1} sent body {2}: {3}", link.Name,
                        InternalHandle, sendBuffer.Buffer.Length,
                        sendBuffer.Buffer.ToHexString());
                }
            }

            Trace.Log("{0} {1} sent {2}/{3} byte(s)",
                link.Name, InternalHandle, bytesTransferred, lengthToSend);

            lock (syncRoot)
            {
                if (eventsToSend.Count == 0)
                {
                    eventsSending.Clear();
                    txFlag = false;
                    return;
                }
            }

            BeginSend();
        }

        protected virtual bool Process(Event e)
        {
            return false;
        }

        protected virtual void OnEventReceived(Event e)
        {
            Diag.IncrementEventsReceived();
        }

        protected virtual void OnEventSent(Event e)
        {
            Diag.IncrementEventsSent();

            if (HasHeartbeatStrategy)
            {
                HeartbeatStrategy.OnSend(e);
            }
        }

        #region Diagnostics

        /// <summary>
        /// Gets or sets the diagnostics object.
        /// </summary>
        public Diagnostics Diag { get; set; }

        /// <summary>
        /// Link session diagnostics helper class.
        /// </summary>
        public class Diagnostics : Link.Diagnostics
        {
            protected LinkSession owner;

            public Diagnostics(LinkSession owner)
            {
                this.owner = owner;
            }

            internal override void AddBytesReceived(long bytesReceived)
            {
                base.AddBytesReceived(bytesReceived);

                if (owner.Link != null)
                {
                    owner.Link.Diag.AddBytesReceived(bytesReceived);
                }
            }

            internal override void AddBytesSent(long bytesSent)
            {
                base.AddBytesSent(bytesSent);

                if (owner.Link != null)
                {
                    owner.Link.Diag.AddBytesSent(bytesSent);
                }
            }

            internal override void IncrementEventsReceived()
            {
                base.IncrementEventsReceived();

                if (owner.Link != null)
                {
                    owner.Link.Diag.IncrementEventsReceived();
                }
            }

            internal override void IncrementEventsSent()
            {
                base.IncrementEventsSent();

                if (owner.Link != null)
                {
                    owner.Link.Diag.IncrementEventsSent();
                }
            }
        }

        #endregion  // Diagnostics
    }
}
