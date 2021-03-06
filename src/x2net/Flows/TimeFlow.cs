﻿// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    public class PriorityQueue<TPriority, TItem>
    {
        private SortedDictionary<TPriority, List<TItem>> store;

        public int Count { get { return store.Count; } }

        public PriorityQueue()
        {
            store = new SortedDictionary<TPriority, List<TItem>>();
        }

        public void Enqueue(TPriority priority, TItem item)
        {
            List<TItem> items;
            if (!store.TryGetValue(priority, out items))
            {
                items = new List<TItem>();
                store.Add(priority, items);
            }
            items.Add(item);
        }

        public TItem Dequeue()
        {
            var first = First();
            var items = first.Value;
            TItem item = items[0];
            items.RemoveAt(0);
            if (items.Count == 0)
            {
                store.Remove(first.Key);
            }
            return item;
        }

        public List<TItem> DequeueBundle()
        {
            var first = First();
            store.Remove(first.Key);
            return first.Value;
        }

        public TPriority Peek()
        {
            var first = First();
            return first.Key;
        }

        public void Remove(TPriority priority, TItem item)
        {
            List<TItem> items;
            if (store.TryGetValue(priority, out items))
            {
                items.Remove(item);
                if (items.Count == 0)
                {
                    store.Remove(priority);
                }
            }
        }

        public void Remove(TItem item)
        {
            var keysToRemove = new List<TPriority>();
            foreach (var pair in store)
            {
                var items = pair.Value;
                for (int i = 0; i < items.Count; ++i)
                {
                    if (items[i].Equals(item))
                    {
                        items.RemoveAt(i--);
                    }
                }
                if (items.Count == 0)
                {
                    keysToRemove.Add(pair.Key);
                }
            }
            for (int i = 0, count = keysToRemove.Count; i < count; ++i)
            {
                store.Remove(keysToRemove[i]);
            }
        }

        // First() extension method workaround to clearly support .NEt 2.0
        private KeyValuePair<TPriority, List<TItem>> First()
        {
            using (var enumerator = store.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

    public class Timer
    {
        private ReaderWriterLockSlim rwlock;
        private PriorityQueue<DateTime, object> reserved;
        private Repeater repeater;

        private readonly TimerCallback callback;

        public Timer(TimerCallback callback)
        {
            rwlock = new ReaderWriterLockSlim();
            reserved = new PriorityQueue<DateTime, object>();
            repeater = new Repeater(this);

            this.callback = callback;
        }

        ~Timer()
        {
            rwlock.Dispose();
        }

        public Token Reserve(object state, double seconds)
        {
            return ReserveAtUniversalTime(state, DateTime.UtcNow.AddSeconds(seconds));
        }

        public Token Reserve(object state, TimeSpan delay)
        {
            return ReserveAtUniversalTime(state, DateTime.UtcNow.Add(delay));
        }

        public Token ReserveAtLocalTime(object state, DateTime localTime)
        {
            return ReserveAtUniversalTime(state, localTime.ToUniversalTime());
        }

        public Token ReserveAtUniversalTime(object state, DateTime universalTime)
        {
            rwlock.EnterWriteLock();
            try
            {
                reserved.Enqueue(universalTime, state);
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
            return new Token(universalTime, state);
        }

        public void Cancel(Token token)
        {
            rwlock.EnterWriteLock();
            try
            {
                reserved.Remove(token.key, token.value);
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public void Cancel(Event e)
        {
            rwlock.EnterWriteLock();
            try
            {
                reserved.Remove(e);
            }
            finally
            {
                rwlock.ExitWriteLock();
            }
        }

        public void ReserveRepetition(object state, TimeSpan interval)
        {
            repeater.Add(state, new Tag(interval));
        }

        public void ReserveRepetition(object state, DateTime nextUtcTime, TimeSpan interval)
        {
            repeater.Add(state, new Tag(nextUtcTime, interval));
        }

        public void CancelRepetition(object state)
        {
            repeater.Remove(state);
        }

        public void Tick()
        {
            DateTime utcNow = DateTime.UtcNow;

            IList<object> events = null;
            rwlock.EnterUpgradeableReadLock();
            try
            {
                if (reserved.Count != 0)
                {
                    DateTime next = reserved.Peek();
                    if (utcNow >= next)
                    {
                        rwlock.EnterWriteLock();
                        try
                        {
                            events = reserved.DequeueBundle();
                        }
                        finally
                        {
                            rwlock.ExitWriteLock();
                        }
                    }
                }
            }
            finally
            {
                rwlock.ExitUpgradeableReadLock();
            }

            if ((object)events != null)
            {
                for (int i = 0; i < events.Count; ++i)
                {
                    callback(events[i]);
                }
            }

            repeater.Tick(utcNow);
        }

        public struct Token
        {
            public DateTime key;
            public object value;

            public Token(DateTime key, object value)
            {
                this.key = key;
                this.value = value;
            }
        }

        private class Tag
        {
            public DateTime NextUtcTime { get; set; }
            public TimeSpan Interval { get; private set; }

            public Tag(TimeSpan interval)
                : this(DateTime.UtcNow + interval, interval)
            {
            }

            public Tag(DateTime nextUtcTime, TimeSpan interval)
            {
                NextUtcTime = nextUtcTime;
                Interval = interval;
            }
        }

        private class Repeater
        {
            private ReaderWriterLockSlim rwlock;
            private Dictionary<object, Tag> map;
            private Timer owner;

            private Tag defaultCase;

            public Repeater(Timer owner)
            {
                rwlock = new ReaderWriterLockSlim();
                map = new Dictionary<object, Tag>();
                this.owner = owner;
            }

            ~Repeater()
            {
                rwlock.Dispose();
            }

            public void Add(object state, Tag timeTag)
            {
                rwlock.EnterWriteLock();
                try
                {
                    if (state != null)
                    {
                        map[state] = timeTag;
                    }
                    else
                    {
                        defaultCase = timeTag;
                    }
                }
                finally
                {
                    rwlock.ExitWriteLock();
                }
            }

            public void Remove(object state)
            {
                rwlock.EnterWriteLock();
                try
                {
                    if (state != null)
                    {
                        map.Remove(state);
                    }
                    else
                    {
                        defaultCase = null;
                    }
                }
                finally
                {
                    rwlock.ExitWriteLock();
                }
            }

            public void Tick(DateTime utcNow)
            {
                rwlock.EnterReadLock();
                try
                {
                    if (defaultCase != null)
                    {
                        TryFire(utcNow, null, defaultCase);
                    }
                    if (map.Count != 0)
                    {
                        foreach (var pair in map)
                        {
                            TryFire(utcNow, pair.Key, pair.Value);
                        }
                    }
                }
                finally
                {
                    rwlock.ExitReadLock();
                }
            }

            private void TryFire(DateTime utcNow, object state, Tag tag)
            {
                if (utcNow >= tag.NextUtcTime)
                {
                    tag.NextUtcTime = tag.NextUtcTime + tag.Interval;
                    owner.callback(state);
                }
            }
        }
    }

    public sealed class TimeFlow
#if NET40
        : FrameBasedFlow<ConcurrentEventQueue>
#else
        : FrameBasedFlow<SynchronizedEventQueue>
#endif
    {
        private Timer timer;

        /// <summary>
        /// Gets the singleton instance of TimeFlow.
        /// </summary>
        public static TimeFlow Instance { get; private set; }

        static TimeFlow()
        {
            Instance = new TimeFlow();

            Instance.Start().Attach();
        }

        private TimeFlow()
        {
            timer = new Timer(OnTimer);
        }

        public Timer.Token Reserve(Event e, double seconds)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }
            return timer.Reserve(e, seconds);
        }
        
        public Timer.Token Reserve(Event e, TimeSpan delay)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }
            return timer.Reserve(e, delay);
        }

        public Timer.Token ReserveAtLocalTime(Event e, DateTime localTime)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }
            return timer.ReserveAtLocalTime(e, localTime);
        }

        public Timer.Token ReserveAtUniversalTime(Event e, DateTime universalTime)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }
            return timer.ReserveAtUniversalTime(e, universalTime);
        }

        public void Cancel(Timer.Token token)
        {
            timer.Cancel(token);
        }

        public void Cancel(Event e)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }
            timer.Cancel(e);
        }

        public void ReserveRepetition(Event e, TimeSpan interval)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }
            timer.ReserveRepetition(e, interval);
        }

        public void ReserveRepetition(Event e, DateTime nextUtcTime, TimeSpan interval)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }
            timer.ReserveRepetition(e, nextUtcTime, interval);
        }

        public void CancelRepetition(Event e)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }
            timer.CancelRepetition(e);
        }

        protected override void Begin() { }
        protected override void End() { }

        protected override void Update()
        {
            timer.Tick();
        }

        void OnTimer(object state)
        {
            Publish((Event)state);
        }
    }
}
