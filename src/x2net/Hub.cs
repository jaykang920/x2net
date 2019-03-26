// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    /// <summary>
    /// Represents the singleton event distribution bus.
    /// </summary>
    public sealed class Hub
    {
        // List of hub cases
        private List<Case> cases;
        // List of all the flows attached to this hub
        private List<Flow> flows;
        // Explicit (named) channel subscription map
        private Dictionary<string, List<Flow>> subscriptions;

        private ReaderWriterLockSlim rwlock;

        /// <summary>
        /// Gets the shared instance of x2 subsystem heartbeat event.
        /// </summary>
        public static HeartbeatEvent HeartbeatEvent { get; private set; }

        /// <summary>
        /// Gets the singleton instance of the hub.
        /// </summary>
        public static Hub Instance { get; private set; }

        static Hub()
        {
            HeartbeatEvent = new HeartbeatEvent { _Transform = false };

            // Initialize the singleton instance.
            Instance = new Hub();
        }

        // Private constructor to prevent explicit instantiation
        private Hub()
        {
            cases = new List<Case>();
            flows = new List<Flow>();
            subscriptions = new Dictionary<string, List<Flow>>();

            rwlock = new ReaderWriterLockSlim();
        }

        ~Hub()
        {
            rwlock.Dispose();
        }

        /// <summary>
        /// Adds the specified hub case to the hub.
        /// </summary>
        public Hub Add(Case c)
        {
            if (ReferenceEquals(c, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (!cases.Contains(c))
                {
                    cases.Add(c);
                    Trace.Debug("hub: added case {0}", c.GetType().Name);
                }
            }
            return this;
        }

        /// <summary>
        /// Attaches the specified flow to the hub.
        /// </summary>
        public Hub Attach(Flow flow)
        {
            if (ReferenceEquals(flow, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (!flows.Contains(flow))
                {
                    flows.Add(flow);
                    Trace.Debug("hub: attached flow {0}", flow.Name);
                }
            }
            return this;
        }

        /// <summary>
        /// Detaches the specified flow from the hub.
        /// </summary>
        public Hub Detach(Flow flow)
        {
            if (ReferenceEquals(flow, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (flows.Remove(flow))
                {
                    UnsubscribeInternal(flow);
                    Trace.Debug("hub: detached flow {0}", flow.Name);
                }
            }
            return this;
        }

        /// <summary>
        /// Detaches all the attached flows.
        /// </summary>
        public void DetachAll()
        {
            using (new WriteLock(rwlock))
            {
                subscriptions.Clear();
                flows.Clear();
                Trace.Debug("hub: detached all the flows");
            }
        }

        private void Feed(Event e)
        {
            if (ReferenceEquals(e, null))
            {
                throw new ArgumentNullException();
            }

            rwlock.EnterReadLock();
            try
            {
                List<Flow> subscribers;
                string channel = e._Channel;

                if (String.IsNullOrEmpty(channel))
                {
                    subscribers = flows;
                }
                else
                {
                    if (!subscriptions.TryGetValue(channel, out subscribers))
                    {
                        return;
                    }
                }
                for (int i = 0, count = subscribers.Count; i < count; ++i)
                {
                    subscribers[i].Feed(e);
                }
            }
            finally
            {
                rwlock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets an array of attached flows by the given name.
        /// </summary>
        public Flow[] GetFlows(string name)
        {
            using (new ReadLock(rwlock))
            {
                List<Flow> matches = new List<Flow>();
                for (int i = 0; i < flows.Count; ++i)
                {
                    var flow = flows[i];
                    if (flow.Name == name)
                    {
                        matches.Add(flow);
                    }
                }
                return matches.ToArray();
            }
        }

        /// <summary>
        /// Gets an array of attached flows by the given type.
        /// </summary>
        public Flow[] GetFlows(Type type)
        {
            using (new ReadLock(rwlock))
            {
                List<Flow> matches = new List<Flow>();
                for (int i = 0; i < flows.Count; ++i)
                {
                    var flow = flows[i];
                    if (flow.GetType() == type)
                    {
                        matches.Add(flow);
                    }
                }
                return matches.ToArray();
            }
        }

        /// <summary>
        /// Inserts the specified hub case to the hub, at the specified order.
        /// </summary>
        public Hub Insert(int index, Case c)
        {
            if (ReferenceEquals(c, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (!cases.Contains(c))
                {
                    if (index < 0) { index = 0; }
                    if (index > cases.Count) { index = cases.Count; }
                    cases.Insert(index, c);
                    Trace.Debug("hub: inserted case {0} at index {1}",
                        c.GetType().Name, index);
                }
            }
            return this;
        }

        /// <summary>
        /// Posts up the specified event to the hub.
        /// </summary>
        public static void Post(Event e)
        {
            Instance.Feed(e);
        }

        /// <summary>
        /// Removes the specified hub case from the hub.
        /// </summary>
        public Hub Remove(Case c)
        {
            if (ReferenceEquals(c, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                cases.Remove(c);
                Trace.Debug("hub: removed case {0}", c.GetType().Name);
            }
            return this;
        }

        private void Setup()
        {
            List<Case> snapshot;
            using (new ReadLock(rwlock))
            {
                snapshot = new List<Case>(cases);
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                var c = snapshot[i];
                Trace.Log("hub: setting up case {0}", c.GetType().Name);
                c.Setup();
            }
        }

        private void StartFlows()
        {
            List<Flow> snapshot;
            using (new ReadLock(rwlock))
            {
                snapshot = new List<Flow>(flows);
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                var flow = snapshot[i];
                Trace.Log("hub: starting flow {0}", flow.Name);
                flow.Start();
                Trace.Debug("hub: started flow {0}", flow.Name);
            }
        }

        /// <summary>
        /// Starts all the flows attached to the hub.
        /// </summary>
        public static void Startup()
        {
            Trace.Debug("starting up");

            Instance.Setup();

            Instance.StartFlows();

            TimeFlow.Instance.ReserveRepetition(HeartbeatEvent,
                new TimeSpan(0, 0, Config.HeartbeatInterval));

            Trace.Info("started");
        }

        private void Teardown()
        {
            List<Case> snapshot;
            using (new ReadLock(rwlock))
            {
                snapshot = new List<Case>(cases);
            }
            for (int i = snapshot.Count - 1; i >= 0; --i)
            {
                try
                {
                    var c = snapshot[i];
                    Trace.Log("hub: tearing down case {0}", c.GetType().Name);
                    c.Teardown();
                }
                catch
                {
                    // Silent here
                }
            }
        }

        private void StopFlows()
        {
            List<Flow> snapshot;
            using (new ReadLock(rwlock))
            {
                snapshot = new List<Flow>(flows);
            }
            for (int i = snapshot.Count - 1; i >= 0; --i)
            {
                try
                {
                    var flow = snapshot[i];
                    Trace.Log("hub: stopping flow {0}", flow.Name);
                    flow.Stop();
                    Trace.Debug("hub: stopped flow {0}", flow.Name);
                }
                catch (Exception e)
                {
                    Trace.Warn("{0} shutdown: {2}", snapshot[i].Name, e);
                }
            }
        }

        /// <summary>
        /// Stops all the flows attached to the hub.
        /// </summary>
        public static void Shutdown()
        {
            Trace.Debug("shutting down");
            try
            {
                TimeFlow.Instance.CancelRepetition(HeartbeatEvent);
                Instance.StopFlows();
            }
            catch (Exception)
            {
                // no-op
            }
            finally
            {
                Trace.Info("stopped");
                Instance.Teardown();  // won't throw
            }
        }

        /// <summary>
        /// Makes the given attached flow subscribe to the specified channel.
        /// </summary>
        internal void Subscribe(Flow flow, string channel)
        {
            if (ReferenceEquals(flow, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (!flows.Contains(flow))
                {
                    throw new InvalidOperationException();
                }
                SubscribeInternal(flow, channel);
            }
        }

        /// <summary>
        /// Makes the given attached flow unsubscribe from the specified channel.
        /// </summary>
        internal void Unsubscribe(Flow flow, string channel)
        {
            if (ReferenceEquals(flow, null))
            {
                throw new ArgumentNullException();
            }
            using (new WriteLock(rwlock))
            {
                if (!flows.Contains(flow))
                {
                    throw new InvalidOperationException();
                }
                UnsubscribeInternal(flow, channel);
            }
        }

        // Lets the given flow subscribe to the specified channel.
        private void SubscribeInternal(Flow flow, string channel)
        {
            if (String.IsNullOrEmpty(channel))
            {
                // invalid channel name
                return;
            }

            flow.AddChannelRef();

            List<Flow> subscribers;
            if (subscriptions.TryGetValue(channel, out subscribers))
            {
                if (subscribers.Contains(flow))
                {
                    return;
                }
            }
            else
            {
                subscribers = new List<Flow>();
                subscriptions.Add(channel, subscribers);
            }
            subscribers.Add(flow);
        }

        // Lets the given flow unsubscribe from all the channels.
        private void UnsubscribeInternal(Flow flow)
        {
            var keysToRemove = new List<string>();

            foreach (var pair in subscriptions)
            {
                var subscribers = pair.Value;
                if (subscribers.Remove(flow))
                {
                    flow.ResetChannelRef();
                    if (subscribers.Count == 0)
                    {
                        keysToRemove.Add(pair.Key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                subscriptions.Remove(key);
            }
        }

        // Lets the given flow unsubscribe from the specified channel.
        private void UnsubscribeInternal(Flow flow, string channel)
        {
            if (String.IsNullOrEmpty(channel))
            {
                // invalid channel name
                return;
            }

            List<Flow> subscribers;
            if (!subscriptions.TryGetValue(channel, out subscribers))
            {
                return;
            }
            int index = subscribers.IndexOf(flow);
            if (index < 0)
            {
                return;
            }
            if (flow.RemoveChannelRef() == 0)
            {
                subscribers.RemoveAt(index);
                if (subscribers.Count == 0)
                {
                    subscriptions.Remove(channel);
                }
            }
        }

        /// <summary>
        /// Represents a hub-scope case that are initialized and terminated
        /// along with startup/shutdown of the hub.
        /// </summary>
        public abstract class Case
        {
            /// <summary>
            /// Overridden by subclasses for initialization.
            /// </summary>
            public virtual void Setup() { }

            /// <summary>
            /// Overridden by subclasses for clean-up.
            /// </summary>
            public virtual void Teardown() { }
        }

        /// <summary>
        /// Represents the set of attached flows for convenient cleanup.
        /// </summary>
        public sealed class Flows : IDisposable
        {
            ~Flows()
            {
                Shutdown();
            }

            /// <summary>
            /// Implements the IDisposable interface.
            /// </summary>
            public void Dispose()
            {
                Shutdown();
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Starts all the attached flows.
            /// </summary>
            public Flows Startup()
            {
                Hub.Startup();
                return this;
            }

            /// <summary>
            /// Stops all the attached flows.
            /// </summary>
            public void Shutdown()
            {
                Hub.Shutdown();
            }
        }
    }
}
