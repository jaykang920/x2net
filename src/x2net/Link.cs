// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Common abstract base class for link cases.
    /// </summary>
    public abstract class Link : Case
    {
        /// <summary>
        /// Indicates whether this link is in the middle of active closing.
        /// </summary>
        protected volatile bool closing;

        private static HashSet<string> names;

        /// <summary>
        /// Gets the link-local event factory.
        /// </summary>
        public EventFactory EventFactory { get; private set; }

        /// <summary>
        /// Gets the name of this link.
        /// </summary>
        public string Name { get; private set; }

        static Link()
        {
            names = new HashSet<string>();
        }

        /// <summary>
        /// Initializes a new instance of the Link class.
        /// </summary>
        protected Link(string name)
        {
            lock (names)
            {
                if (names.Contains(name))
                {
                    throw new ArgumentException(String.Format(
                        "link name {0} is already in use", name));
                }
                names.Add(name);
            }

            EventFactory = new EventFactory();
            Name = name;
        }

        ~Link()
        {
            Dispose(false);
        }

        /// <summary>
        /// Closes this link and releases all the associated resources.
        /// </summary>
        public void Close()
        {
            closing = true;

            Dispose();
        }

        /// <summary>
        /// Creates a new event instance based on the specified type identifier.
        /// </summary>
        public Event CreateEvent(int typeId)
        {
            // Try link-local event factory first.
            Event result = EventFactory.Create(typeId);
            if (!ReferenceEquals(result, null))
            {
                return result;
            }
            // If not fount, try the global event factory.
            return EventFactory.Global.Create(typeId);
        }

        /// <summary>
        /// Sends out the specified event through this link channel.
        /// </summary>
        public abstract void Send(Event e);

        /// <summary>
        /// Frees managed or unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            lock (names)
            {
                names.Remove(Name);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// <see cref="Case.TeardownInternal"/>
        /// </summary>
        protected override void TeardownInternal()
        {
            Close();

            base.TeardownInternal();
        }

        #region Diagnostics

        /// <summary>
        /// Gets or sets the diagnostics object.
        /// </summary>
        public Diagnostics Diag { get; set; }

        /// <summary>
        /// Link diagnostics helper class.
        /// </summary>
        public class Diagnostics
        {
            protected long totalBytesReceived;
            protected long totalBytesSent;
            protected long bytesReceived;
            protected long bytesSent;

            protected long totalEventsReceived;
            protected long totalEventsSent;
            protected long eventsReceived;
            protected long eventsSent;

            public long TotalBytesReceived
            {
                get { return Interlocked.Read(ref totalBytesReceived); }
            }

            public long TotalBytesSent
            {
                get { return Interlocked.Read(ref totalBytesSent); }
            }

            public long BytesReceived
            {
                get { return Interlocked.Read(ref bytesReceived); }
            }

            public long BytesSent
            {
                get { return Interlocked.Read(ref bytesSent); }
            }

            public long TotalEventsReceived
            {
                get { return Interlocked.Read(ref totalEventsReceived); }
            }

            public long TotalEventsSent
            {
                get { return Interlocked.Read(ref totalEventsSent); }
            }

            public long EventsReceived
            {
                get { return Interlocked.Read(ref eventsReceived); }
            }

            public long EventsSent
            {
                get { return Interlocked.Read(ref eventsSent); }
            }

            internal virtual void AddBytesReceived(long bytesReceived)
            {
                Interlocked.Add(ref totalBytesReceived, bytesReceived);
                Interlocked.Add(ref this.bytesReceived, bytesReceived);
            }

            internal virtual void AddBytesSent(long bytesSent)
            {
                Interlocked.Add(ref totalBytesSent, bytesSent);
                Interlocked.Add(ref this.bytesSent, bytesSent);
            }

            public void ResetBytesReceived()
            {
                Interlocked.Exchange(ref bytesReceived, 0L);
            }

            public void ResetBytesSent()
            {
                Interlocked.Exchange(ref bytesSent, 0L);
            }

            internal virtual void IncrementEventsReceived()
            {
                Interlocked.Increment(ref totalEventsReceived);
                Interlocked.Increment(ref eventsReceived);
            }

            internal virtual void IncrementEventsSent()
            {
                Interlocked.Increment(ref totalEventsSent);
                Interlocked.Increment(ref eventsSent);
            }

            public void ResetEventsReceived()
            {
                Interlocked.Exchange(ref eventsReceived, 0L);
            }

            public void ResetEventsSent()
            {
                Interlocked.Exchange(ref eventsSent, 0L);
            }
        }

        #endregion  // Diagnostics
    }
}
