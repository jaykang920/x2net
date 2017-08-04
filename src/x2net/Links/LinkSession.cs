// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Abstract base class for concrete link sessions.
    /// </summary>
    public abstract class LinkSession : IDisposable
    {
        protected int handle;
        protected SessionBasedLink link;
        protected bool polarity;

        protected Buffer rxBuffer;
        protected List<ArraySegment<byte>> rxBufferList;
        protected List<ArraySegment<byte>> txBufferList;

        protected List<Event> eventsSending;
        protected List<Event> eventsToSend;
        protected List<SendBuffer> buffersSending;
        protected List<SendBuffer> buffersSent;

        protected int lengthToReceive;
        protected int lengthToSend;

        protected bool rxBeginning;

        protected bool rxTransformed;
        protected bool rxTransformReady;
        protected bool txTransformReady;

        protected bool txFlag;

        protected object syncRoot = new Object();

        protected volatile bool closing;
        protected volatile bool disposed;

        protected long rxCounter;
        protected long txCounter;
        protected long txCompleted;

        protected volatile bool connected;

        private List<Event> preConnectionQueue;

        /// <summary>
        /// Gets or sets the BufferTransform for this link session.
        /// </summary>
        public IBufferTransform BufferTransform { get; set; }

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
        /// Gets or sets the session token for this session.
        /// </summary>
        public string Token { get; set; }

        public long RxCounter
        {
            get { return Interlocked.Read(ref rxCounter); }
        }

        public long TxCounter
        {
            get { return Interlocked.Read(ref txCounter); }
        }

        public long TxCompleted
        {
            get { return Interlocked.Read(ref txCompleted); }
        }

        public int TxBuffered
        {
            get
            {
                lock (syncRoot)
                {
                    return buffersSending.Count + buffersSent.Count;
                }
            }
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

        /// <summary>
        /// Initializes a new instance of the LinkSession class.
        /// </summary>
        protected LinkSession(SessionBasedLink link)
        {
            this.link = link;

            rxBuffer = new Buffer();
            rxBufferList = new List<ArraySegment<byte>>();
            txBufferList = new List<ArraySegment<byte>>();

            eventsSending = new List<Event>();
            eventsToSend = new List<Event>();
            buffersSending = new List<SendBuffer>();
            buffersSent = new List<SendBuffer>();

            if (link.SessionRecoveryEnabled)
            {
                preConnectionQueue = new List<Event>();
            }

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
            closing = true;

            if (link.SessionRecoveryEnabled)
            {
                Send(new SessionEnd { _Transform = false });
            }

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

        public void Release()
        {
            if (!Object.ReferenceEquals(BufferTransform, null))
            {
                BufferTransform.Dispose();
                BufferTransform = null;
            }

            for (int i = 0, count = buffersSending.Count; i < count; ++i)
            {
                buffersSending[i].Dispose();
            }
            buffersSending.Clear();

            for (int i = 0, count = buffersSent.Count; i < count; ++i)
            {
                buffersSent[i].Dispose();
            }
            buffersSent.Clear();
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

            if (link.SessionRecoveryEnabled == false &&
                !Object.ReferenceEquals(BufferTransform, null))
            {
                BufferTransform.Dispose();
                BufferTransform = null;
            }

            txBufferList.Clear();
            rxBufferList.Clear();
            rxBuffer.Dispose();

            if (link.SessionRecoveryEnabled)
            {
                // Postpone disposing send buffers

                preConnectionQueue.Clear();
            }
            else
            {
                for (int i = 0, count = buffersSending.Count; i < count; ++i)
                {
                    buffersSending[i].Dispose();
                }
                buffersSending.Clear();

                for (int i = 0, count = buffersSent.Count; i < count; ++i)
                {
                    buffersSent[i].Dispose();
                }
                buffersSent.Clear();
            }
        }

        /// <summary>
        /// Sends out the specified event through this link session.
        /// </summary>
        public void Send(Event e)
        {
            if (disposed && !link.SessionRecoveryEnabled)
            {
                Trace.Warn("{0} {1} dropped {2}", link.Name, InternalHandle, e);
                return;
            }

            lock (syncRoot)
            {
                if (link.SessionRecoveryEnabled && !disposed &&
                    !connected && e.GetTypeId() > 0)  // not builtin events
                {
                    Trace.Info("{0} {1} pre-establishment buffered {2}",
                        link.Name, InternalHandle, e);
                    preConnectionQueue.Add(e);
                    return;
                }

                eventsToSend.Add(e);

                if (txFlag || disposed)
                {
                    //Trace.Debug("{0} {1} buffered {2}", link.Name, InternalHandle, e);
                    return;
                }

                txFlag = true;
            }

            BeginSend();
        }

        /// <summary>
        /// Sends out the specified events through this link session.
        /// </summary>
        public void Send(Event[] events)
        {
            if (disposed && !link.SessionRecoveryEnabled)
            {
                for (int i = 0; i < events.Length; ++i)
                {
                    Trace.Warn("{0} {1} dropped {2}", link.Name, InternalHandle, events[i]);
                }
                return;
            }

            lock (syncRoot)
            {
                if (link.SessionRecoveryEnabled && !disposed &&
                    !connected)  // not builtin events
                {
                    for (int i = 0; i < events.Length; ++i)
                    {
                        Event e = events[i];
                        if (e.GetTypeId() <= 0) { continue; }

                        Trace.Info("{0} {1} pre-establishment buffered {2}",
                            link.Name, InternalHandle, e);

                        preConnectionQueue.Add(e);
                    }

                    return;
                }

                for (int i = 0; i < events.Length; ++i)
                {
                    eventsToSend.Add(events[i]);
                }

                if (txFlag || disposed)
                {
                    //Trace.Debug("{0} {1} buffered {2}", link.Name, InternalHandle, e);
                    return;
                }

                txFlag = true;
            }

            BeginSend();
        }

        internal void InheritFrom(LinkSession oldSession)
        {
            lock (syncRoot)
            {
                handle = oldSession.Handle;
                Token = oldSession.Token;

                Trace.Debug("{0} {1} session inheritance {2}",
                    link.Name, handle, Token);

                BufferTransform = oldSession.BufferTransform;
                rxTransformReady = oldSession.rxTransformReady;
                txTransformReady = oldSession.txTransformReady;
            }
        }

        public void TakeOver(LinkSession oldSession, int retransmission)
        {
            if (retransmission == 0)
            {
                lock (syncRoot)
                {
                    lock (oldSession.syncRoot)
                    {
                        if (oldSession.buffersSending.Count != 0)
                        {
                            // Dispose them
                            for (int i = 0, count = oldSession.buffersSending.Count; i < count; ++i)
                            {
                                oldSession.buffersSending[i].Dispose();
                            }
                            oldSession.buffersSending.Clear();
                        }
                        if (oldSession.buffersSent.Count != 0)
                        {
                            // Dispose them
                            for (int i = 0, count = oldSession.buffersSent.Count; i < count; ++i)
                            {
                                oldSession.buffersSent[i].Dispose();
                            }
                            oldSession.buffersSent.Clear();
                        }

                        Trace.Info("{0} {1} pre-establishment buffered {2}",
                            link.Name, InternalHandle, preConnectionQueue.Count);

                        if (preConnectionQueue.Count != 0)
                        {
                            eventsToSend.InsertRange(0, preConnectionQueue);
                            preConnectionQueue.Clear();
                        }
                        if (oldSession.eventsToSend.Count != 0)
                        {
                            eventsToSend.InsertRange(0, oldSession.eventsToSend);
                            oldSession.eventsToSend.Clear();
                        }

                        Trace.Info("{0} {1} eventsToSend {2}", link.Name, InternalHandle, eventsToSend.Count);
                    }

                    connected = true;

                    if (txFlag || eventsToSend.Count == 0)
                    {
                        return;
                    }

                    txFlag = true;
                }

                BeginSend();
            }
            else
            {
                Monitor.Enter(syncRoot);
                try
                {
                    // Wait for any existing send to complete
                    while (txFlag)
                    {
                        Monitor.Exit(syncRoot);
                        Thread.Sleep(1);
                        Monitor.Enter(syncRoot);
                    }

                    txBufferList.Clear();
                    lengthToSend = 0;
                    buffersSending.Clear();

                    lock (oldSession.syncRoot)
                    {
                        List<SendBuffer> buffers1, buffers2;

                        if (oldSession.TxCounter == oldSession.TxCompleted)
                        {
                            buffers1 = oldSession.buffersSending;
                            buffers2 = oldSession.buffersSent;
                        }
                        else
                        {
                            buffers1 = oldSession.buffersSent;
                            buffers2 = oldSession.buffersSending;
                        }

                        int numBuffers1ToDispose = oldSession.TxBuffered - retransmission;
                        if (numBuffers1ToDispose > buffers1.Count)
                        {
                            numBuffers1ToDispose = buffers1.Count;
                        }
                        int numBuffers2ToDispose = buffers2.Count - retransmission;
                        if (numBuffers2ToDispose < 0)
                        {
                            numBuffers2ToDispose = 0;
                        }
                        
                        int i;

                        for (i = 0; i < numBuffers1ToDispose; ++i)
                        {
                            buffers1[i].Dispose();
                        }
                        for (; i < buffers1.Count; ++i)
                        {
                            SendBuffer buffer = buffers1[i];
                            buffersSending.Add(buffer);
                            buffer.ListOccupiedSegments(txBufferList);
                            lengthToSend += buffer.Length;
                        }
                        buffers1.Clear();

                        for (i = 0; i < numBuffers2ToDispose; ++i)
                        {
                            buffers2[i].Dispose();
                        }
                        for (; i < buffers2.Count; ++i)
                        {
                            SendBuffer buffer = buffers2[i];
                            buffersSending.Add(buffer);
                            buffer.ListOccupiedSegments(txBufferList);
                            lengthToSend += buffer.Length;
                        }
                        buffers2.Clear();

                        Trace.Info("{0} {1} pre-establishment buffered {2}",
                            link.Name, InternalHandle, preConnectionQueue.Count);

                        if (preConnectionQueue.Count != 0)
                        {
                            eventsToSend.InsertRange(0, preConnectionQueue);
                            preConnectionQueue.Clear();
                        }
                        if (oldSession.eventsToSend.Count != 0)
                        {
                            eventsToSend.InsertRange(0, oldSession.eventsToSend);
                            oldSession.eventsToSend.Clear();
                        }

                        Trace.Info("{0} {1} eventsToSend {2}", link.Name, InternalHandle, eventsToSend.Count);
                    }

                    Trace.Warn("{0} {1} retransmitting {2} events ({3} bytes)",
                        link.Name, InternalHandle, retransmission, lengthToSend);

                    connected = true;

                    txFlag = true;
                }
                finally
                {
                    Monitor.Exit(syncRoot);
                }

                Interlocked.Add(ref txCounter, retransmission);

                SendInternal();
            }

            oldSession = null;
            retransmission = 0;
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
            int bufferCount = buffersSending.Count;
            if (bufferCount < count)
            {
                for (int i = 0, n = count - bufferCount; i < n; ++i)
                {
                    buffersSending.Add(new SendBuffer());
                }
            }
            else
            {
                for (int i = 0, n = bufferCount - count; i < n; ++i)
                {
                    int j = bufferCount - (i + 1);
                    buffersSending[j].Dispose();
                    buffersSending.RemoveAt(j);
                }
            }
            for (int i = 0; i < count; ++i)
            {
                Event e = eventsSending[i];

                var sendBuffer = buffersSending[i];
                sendBuffer.Reset();

                int typeId = e.GetTypeId();

                Trace.Log("{0} {1} buffering event type {2} to send (#{3}/{4})",
                    link.Name, InternalHandle, typeId, i + 1, count);

                Serializer serializer = new Serializer(sendBuffer.Buffer);
                serializer.Write(typeId);
                e.Serialize(serializer);

                bool transformed = false;
                if (BufferTransform != null && txTransformReady && e._Transform)
                {
                    BufferTransform.Transform(sendBuffer.Buffer, (int)sendBuffer.Buffer.Length);
                    transformed = true;
                }

                BuildHeader(sendBuffer, transformed);

                sendBuffer.ListOccupiedSegments(txBufferList);
                lengthToSend += sendBuffer.Length;

                OnEventSent(e);
            }

            lock (syncRoot)
            {
                Trace.Log("{0} {1} serialized {2} events to send ({3} bytes)",
                    link.Name, InternalHandle, count, lengthToSend);

                Interlocked.Add(ref txCounter, count);
            }

            SendInternal();
        }

        protected abstract void BuildHeader(SendBuffer sendBuffer, bool transformed);
        protected abstract bool ParseHeader();

        protected abstract void ReceiveInternal();
        protected abstract void SendInternal();

        protected void OnReceiveInternal(int bytesTransferred)
        {
            Diag.AddBytesReceived(bytesTransferred);

            if (disposed)
            {
                return;
            }

            lock (syncRoot)
            {
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

                if (BufferTransform != null && rxTransformReady && rxTransformed)
                {
                    try
                    {
                        BufferTransform.InverseTransform(rxBuffer, lengthToReceive);
                    }
                    catch (Exception e)
                    {
                        Trace.Error("{0} {1} buffer inv transform error: {2}",
                            link.Name, InternalHandle, e.Message);
                        goto next;
                    }
                }
                rxBuffer.Rewind();

                Interlocked.Increment(ref rxCounter);

                var deserializer = new Deserializer(rxBuffer);
                Event retrieved = deserializer.Create();
                if ((object)retrieved == null)
                {
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
                        Trace.Error("{0} {1} error loading event {2}: {3}",
                            link.Name, InternalHandle, retrieved.GetTypeId(), e.ToString());
                        goto next;
                    }

                    OnEventReceived(retrieved);

                    if (!Process(retrieved))
                    {
                        retrieved._Handle = Handle;

                        link.OnPreprocess(this, retrieved);

                        Hub.Post(retrieved);
                    }
                }
            next:
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
            Trace.Debug("{0} {1} OnDisconnect", link.Name, InternalHandle);

            if (handle <= 0)
            {
                return;
            }

            if (link.SessionRecoveryEnabled && !closing)
            {
                lock (syncRoot)
                {
                    link.OnInstantDisconnect(this);
                }
            }
            else
            {
                link.OnLinkSessionDisconnectedInternal(handle, context);
            }
        }

        protected void OnSendInternal(int bytesTransferred)
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
                for (int i = 0; i < buffersSending.Count; ++i)
                {
                    SendBuffer sendBuffer = buffersSending[i];

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
                // Swap send buffers.
                List<SendBuffer> temp = buffersSending;
                buffersSending = buffersSent;
                buffersSent = temp;
                temp = null;

                Interlocked.Add(ref txCompleted, buffersSent.Count);

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
            switch (e.GetTypeId())
            {
                case (int)LinkEventType.HandshakeReq:
                    {
                        var req = (HandshakeReq)e;
                        var resp = new HandshakeResp { _Transform = false };
                        byte[] response = null;
                        try
                        {
                            ManualResetEvent waitHandle =
                                LinkWaitHandlePool.Acquire(InternalHandle);
                            waitHandle.WaitOne(new TimeSpan(0, 0, 30));
                            LinkWaitHandlePool.Release(InternalHandle);
                            response = BufferTransform.Handshake(req.Data);
                        }
                        catch (Exception ex)
                        {
                            Trace.Error("{0} {1} error handshaking : {2}",
                                link.Name, InternalHandle, ex.ToString());
                        }
                        if (response != null)
                        {
                            resp.Data = response;
                        }
                        Send(resp);
                    }
                    break;
                case (int)LinkEventType.HandshakeResp:
                    {
                        var ack = new HandshakeAck { _Transform = false };
                        var resp = (HandshakeResp)e;
                        try
                        {
                            if (BufferTransform.FinalizeHandshake(resp.Data))
                            {
                                rxTransformReady = true;
                                ack.Result = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.Error("{0} {1} error finishing handshake : {2}",
                                link.Name, InternalHandle, ex.ToString());
                        }
                        Send(ack);
                    }
                    break;
                case (int)LinkEventType.HandshakeAck:
                    {
                        var ack = (HandshakeAck)e;
                        bool result = ack.Result;

                        if (result)
                        {
                            txTransformReady = true;
                        }

                        link.OnLinkSessionConnectedInternal(result, (result ? this : null));
                    }
                    break;
                case (int)LinkEventType.SessionReq:
                    if (link.SessionRecoveryEnabled && polarity == false)
                    {
                        var server = (ServerLink)link;
                        server.OnSessionReq(this, (SessionReq)e);
                    }
                    break;
                case (int)LinkEventType.SessionResp:
                    if (link.SessionRecoveryEnabled && polarity == true)
                    {
                        var client = (ClientLink)link;
                        client.OnSessionResp(this, (SessionResp)e);
                    }
                    break;
                case (int)LinkEventType.SessionAck:
                    if (link.SessionRecoveryEnabled && polarity == false)
                    {
                        var server = (ServerLink)link;
                        server.OnSessionAck(this, (SessionAck)e);
                    }
                    break;
                case (int)LinkEventType.SessionEnd:
                    if (link.SessionRecoveryEnabled)
                    {
                        closing = true;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        protected virtual void OnEventReceived(Event e)
        {
            Diag.IncrementEventsReceived();
        }

        protected virtual void OnEventSent(Event e)
        {
            Diag.IncrementEventsSent();
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
