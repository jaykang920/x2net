// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Manages evnet-handler bindings.
    /// </summary>
    public class Binder
    {
        private Dictionary<Event, HandlerSet> handlerMap;
        private Filter filter;

        private ReaderWriterLockSlim rwlock;

        public Binder()
        {
            handlerMap = new Dictionary<Event, HandlerSet>();
            filter = new Filter();

            rwlock = new ReaderWriterLockSlim();

            Diag = new Diagnostics(this);
        }

        ~Binder()
        {
            rwlock.Dispose();
        }

        public void Bind(Token token)
        {
            Bind(token.Key, token.Value);
        }

        public Token Bind(Event e, Handler handler)
        {
            rwlock.EnterWriteLock();
            try
            {
                HandlerSet handlers;
                if (!handlerMap.TryGetValue(e, out handlers))
                {
                    handlers = new HandlerSet();
                    handlerMap.Add(e, handlers);
                }

                var token = new Token(e, handler);
                if (handlers.Add(handler))
                {
                    filter.Add(e.GetTypeId(), e.GetFingerprint());

                    var eventSink = handler.Action.Target as EventSink;
                    if (!ReferenceEquals(eventSink, null))
                    {
                        eventSink.AddBinding(token);
                    }
                }
                return token;
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public int BuildHandlerChain(Event e,
            EventEquivalent equivalent, List<Handler> handlerChain)
        {
            rwlock.EnterReadLock();
            try
            {
                Event.Tag tag = (Event.Tag)e.GetTypeTag();
                Fingerprint fingerprint = e.GetFingerprint();
                while (tag != null)
                {
                    int typeId = tag.TypeId;
                    IList<Slot> slots = filter.Get(typeId);
                    if (slots != null)
                    {
                        for (int i = 0, count = slots.Count; i < count; ++i)
                        {
                            var slot = slots[i];
                            if (slot.Equivalent(fingerprint))
                            {
                                equivalent.InnerEvent = e;
                                equivalent.SetFingerprint(slot);
                                equivalent.InnerTypeId = typeId;

                                HandlerSet handlers;
                                if (handlerMap.TryGetValue(equivalent, out handlers))
                                {
                                    IList<Handler> list = handlers.GetList();
                                    for (int j = 0, jCount = list.Count; j < jCount; ++j)
                                    {
                                        handlerChain.Add(list[j]);
                                    }
                                }
                            }
                        }
                    }
                    tag = (Event.Tag)tag.Base;
                }
                // sort result
                return handlerChain.Count;
            }
            finally
            {
                rwlock.ExitReadLock();
            }
        }

        public void Unbind(Token token)
        {
            Unbind(token.Key, token.Value);
        }

        internal void UnbindInternal(Token token)
        {
            rwlock.EnterWriteLock();
            try
            {
                UnbindInternal(token.Key, token.Value);
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public Binder.Token Unbind(Event e, Handler handler)
        {
            rwlock.EnterWriteLock();
            try
            {
                UnbindInternal(e, handler);

                var token = new Token(e, handler);
                var eventSink = handler.Action.Target as EventSink;
                if (!ReferenceEquals(eventSink, null))
                {
                    eventSink.RemoveBinding(new Token(e, handler));
                }
                return token;
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        private void UnbindInternal(Event e, Handler handler)
        {
            HandlerSet handlers;
            if (!handlerMap.TryGetValue(e, out handlers))
            {
                return;
            }
            if (!handlers.Remove(handler))
            {
                return;
            }
            if (handlers.Count == 0)
            {
                handlerMap.Remove(e);
            }
            filter.Remove(e.GetTypeId(), e.GetFingerprint());
        }

        public struct Token
        {
            private Event key;
            private Handler value;

            public Event Key { get { return key; } }
            public Handler Value { get { return value; } }

            public override bool Equals(object obj)
            {
                if (!(obj is Token))
                {
                    return false;
                }

                Token other = (Token)obj;
                if (!key.Equals(other.key) || !value.Equals(other.value))
                {
                    return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                Hash hash = new Hash(Hash.Seed);
                hash.Update(key.GetHashCode());
                hash.Update(value.GetHashCode());
                return hash.Code;
            }

            public Token(Event key, Handler value)
            {
                this.key = key;
                this.value = value;
            }

            public static bool operator ==(Token x, Token y)
            {
                return x.Equals(y);
            }

            public static bool operator !=(Token x, Token y)
            {
                return !x.Equals(y);
            }
        }

        private class Filter
        {
            private Dictionary<int, List<Slot>> map;

            internal Filter()
            {
                map = new Dictionary<int, List<Slot>>();
            }

            internal void Add(int typeId, Fingerprint fingerprint)
            {
                List<Slot> slots;
                if (map.TryGetValue(typeId, out slots) == false)
                {
                    slots = new List<Slot>();
                    map.Add(typeId, slots);
                }
                Slot slot = new Slot(fingerprint);
                int index = slots.BinarySearch(slot);
                if (index >= 0)
                {
                    slots[index].AddRef();
                }
                else
                {
                    index = ~index;
                    slots.Insert(index, slot);
                }
            }

            internal IList<Slot> Get(int typeId)
            {
                List<Slot> slots;
                map.TryGetValue(typeId, out slots);
                return slots;
            }

            internal void Remove(int typeId, Fingerprint fingerprint)
            {
                List<Slot> slots;
                if (map.TryGetValue(typeId, out slots) == false)
                {
                    return;
                }
                int index = slots.BinarySearch(new Slot(fingerprint));
                if (index >= 0)
                {
                    if (slots[index].RemoveRef() == 0)
                    {
                        slots.RemoveAt(index);
                    }
                    if (slots.Count == 0)
                    {
                        map.Remove(typeId);
                    }
                }
            }
        }

        private class HandlerSet
        {
            private IList<Handler> handlers;

            public int Count { get { return handlers.Count; } }

            public HandlerSet()
            {
                handlers = new List<Handler>();
            }

            public bool Add(Handler handler)
            {
                if (handlers.Contains(handler))
                {
                    return false;
                }
                handlers.Add(handler);
                return true;
            }

            public IList<Handler> GetList()
            {
                return handlers;
            }

            public bool Remove(Handler handler)
            {
                return handlers.Remove(handler);
            }
        }

        #region Diagnostics

        /// <summary>
        /// Gets the diagnostics object.
        /// </summary>
        public Diagnostics Diag { get; private set; }

        /// <summary>
        /// Internal diagnostics helper class.
        /// </summary>
        public class Diagnostics
        {
            private Binder owner;

            internal Diagnostics(Binder owner)
            {
                this.owner = owner;
            }
        }

        #endregion
    }

    /// <summary>
    /// Extends Fingerprint class to hold an additional reference count.
    /// </summary>
    internal class Slot : Fingerprint, IComparable<Slot>
    {
        private int refCount;

        /// <summary>
        /// Initializes a new instance of the Slot class that contains bit values
        /// copied from the specified Fingerprint.
        /// </summary>
        /// <param name="fingerprint">A Fingerprint object to copy from.</param>
        public Slot(Fingerprint fingerprint)
            : base(fingerprint)
        {
            AddRef();  // begin with 1
        }

        public int AddRef()
        {
            return Interlocked.Increment(ref refCount);
        }

        /// <summary>
        /// Compares this Slot with the specified Slot object.
        /// </summary>
        /// Implements IComparable(T).CompareTo interface.
        /// <param name="other">
        /// A Slot object to be compared with this.
        /// </param>
        /// <returns>
        /// A value that indicates the relative order of the Slot objects being
        /// compared. Zero return value means that this is equal to <c>other</c>,
        /// while negative(positive) integer return value means that this is
        /// less(greater) than <c>other</c>.
        /// </returns>
        public int CompareTo(Slot other)
        {
            return base.CompareTo(other);
        }

        public int RemoveRef()
        {
            return Interlocked.Decrement(ref refCount);
        }
    }
}
