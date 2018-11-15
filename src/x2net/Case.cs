// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// Defines methods to initialize/finalize a case. 
    /// </summary>
    public interface ICase
    {
        /// <summary>
        /// Initializes this case with the specified holding flow.
        /// </summary>
        void Setup(Flow holder);
        /// <summary>
        /// Cleans up this case with the specified holding flow.
        /// </summary>
        void Teardown(Flow holder);

        /// <summary>
        /// Called after the holding flow starts.
        /// </summary>
        void Start();
        /// <summary>
        /// Called before the holding flow stops.
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// Represents a finite set of application logic.
    /// </summary>
    public abstract class Case : EventSink, ICase
    {
        /// <summary>
        /// <see cref="ICase.Setup"/>
        /// </summary>
        public void Setup(Flow holder)
        {
            Flow = holder;

            Flow backup = Flow.CurrentFlow;
            Flow.CurrentFlow = holder;

            SetupInternal();

            Flow.CurrentFlow = backup;
        }

        /// <summary>
        /// <see cref="ICase.Teardown"/>
        /// </summary>
        public void Teardown(Flow holder)
        {
            Flow backup = Flow.CurrentFlow;
            Flow.CurrentFlow = holder;

            TeardownInternal();

            Flow.CurrentFlow = backup;

            Dispose();  // EventSink.Dispose()
        }

        /// <summary>
        /// <see cref="ICase.Start"/>
        /// </summary>
        public void Start()
        {
            OnStart();
        }

        /// <summary>
        /// <see cref="ICase.Stop"/>
        /// </summary>
        public void Stop()
        {
            OnStop();
        }

        /// <summary>
        /// Overridden by subclasses to build an initialization chain.
        /// </summary>
        protected virtual void Setup() { }

        /// <summary>
        /// Called internally when this case is initialized.
        /// </summary>
        protected virtual void SetupInternal()
        {
            Setup();
        }

        /// <summary>
        /// Overridden by subclasses to build a cleanup chain.
        /// </summary>
        protected virtual void Teardown() { }

        /// <summary>
        /// Called internally when this case is cleaned up.
        /// </summary>
        protected virtual void TeardownInternal()
        {
            Teardown();
        }

        /// <summary>
        /// Overridden by subclasses to build a flow startup handler chain.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// Overridden by subclasses to build a flow shutdown handler chain.
        /// </summary>
        protected virtual void OnStop() { }
    }

    public class CaseStack : ICase
    {
        private List<ICase> cases;
        private bool activated;

        public CaseStack()
        {
            cases = new List<ICase>();
        }

        public void Add(ICase c)
        {
            lock (cases)
            {
                if (!cases.Contains(c))
                {
                    cases.Add(c);
                }
            }
        }

        public void Remove(ICase c)
        {
            lock (cases)
            {
                cases.Remove(c);
            }
        }

        public void Setup(Flow holder)
        {
            List<ICase> snapshot;
            lock (cases)
            {
                if (activated) { return; }
                activated = true;
                snapshot = new List<ICase>(cases);
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                var c = snapshot[i];
                Trace.Log("casestack: setting up case {0}", c.GetType().Name);
                c.Setup(holder);
            }
        }

        public void Teardown(Flow holder)
        {
            List<ICase> snapshot;
            lock (cases)
            {
                if (!activated) { return; }
                activated = false;
                snapshot = new List<ICase>(cases);
            }
            for (int i = snapshot.Count - 1; i >= 0; --i)
            {
                try
                {
                    var c = snapshot[i];
                    Trace.Log("casestack: tearing down case {0}", c.GetType().Name);
                    c.Teardown(holder);
                }
                catch (Exception e)
                {
                    Trace.Error("{0} {1} Teardown: {2}",
                        holder.Name, snapshot[i].GetType().Name, e);
                }
            }
        }

        /// <summary>
        /// <see cref="ICase.Start"/>
        /// </summary>
        public void Start()
        {
            List<ICase> snapshot;
            lock (cases)
            {
                snapshot = new List<ICase>(cases);
            }
            for (int i = 0, count = snapshot.Count; i < count; ++i)
            {
                snapshot[i].Start();
            }
        }

        /// <summary>
        /// <see cref="ICase.Stop"/>
        /// </summary>
        public void Stop()
        {
            List<ICase> snapshot;
            lock (cases)
            {
                snapshot = new List<ICase>(cases);
            }
            for (int i = snapshot.Count - 1; i >= 0; --i)
            {
                try
                {
                    snapshot[i].Stop();
                }
                catch
                {
                    // Silent here
                }
            }
        }
    }
}
