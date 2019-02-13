// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

#if NET45

using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace x2net
{
    /// <summary>
    /// Named pipe link sessions.
    /// </summary>
    public class NamedPipeSession : LinkSession
    {
        protected PipeStream stream;

        protected internal override int InternalHandle
        {
            get
            {
                if (handle == 0)
                {
                    return -(stream.SafePipeHandle.DangerousGetHandle().ToInt32());
                }
                return base.InternalHandle;
            }
        }

        /// <summary>
        /// Gets the underlying pipe stream.
        /// </summary>
        public PipeStream Stream { get { return stream; } }

        /// <summary>
        /// Initializes a new instance of the NamedPipeSession class.
        /// </summary>
        public NamedPipeSession(SessionBasedLink link, PipeStream stream)
            : base(link)
        {
            this.stream = stream;
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
            return String.Format("{0} {1}",
                GetType().Name, InternalHandle);
        }

        protected override void OnClose()
        {
            OnDisconnect(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            if (stream != null)
            {
                try
                {
                    stream.Close();
                }
                catch (Exception e)
                {
                    Trace.Warn("{0} {1} close : {2}",
                        Link.Name, InternalHandle, e.Message);
                }
                finally
                {
                    stream = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void ReceiveInternal()
        {
            try
            {
                rxBufferList.Clear();
                rxBuffer.ListAvailableSegments(rxBufferList);

                var buffer = rxBufferList[0];
                Task.Run(() => {
                    int result = stream.Read(buffer.Array, buffer.Offset, buffer.Count);
                    if (result == 0)
                    {
                        OnDisconnect();
                    }
                    OnReceiveInternal(result);
                });
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Trace.Info("{0} {1} recv error {2}", link.Name, InternalHandle, e);

                OnDisconnect();
            }
        }

        internal protected override void SendInternal()
        {
            Trace.Info("NamedPipeSession: SendInternal");

            try
            {
                var buffers = txBufferList;

                Task.Run(() => {
                    Trace.Info("NamedPipeSession: SendInternal task");

                    int bytes = 0;
                    try
                    {
                        for (int i = 0; i < buffers.Count; ++i)
                        {
                            var buffer = buffers[i];
                            stream.Write(buffer.Array, buffer.Offset, buffer.Count);
                            bytes += buffer.Count;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Error(ex.ToString());
                    }
                    finally
                    {
                        OnSendInternal(bytes);
                    }
                });
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Trace.Info("{0} {1} send error {2}", link.Name, InternalHandle, e);

                OnDisconnect();
            }
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

#endif
