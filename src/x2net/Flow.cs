// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using System.Reflection;

namespace x2net
{
    /// <summary>
    /// Represents a logically independent execution flow.
    /// </summary>
    public abstract class Flow
    {
        [ThreadStatic]
        protected static Flow currentFlow;
        [ThreadStatic]
        protected static EventEquivalent equivalent;
        [ThreadStatic]
        protected static List<Handler> handlerChain;

        protected Binder binder;
        protected CaseStack caseStack;
        protected string name;

        private int channelRefCount;

        /// <summary>
        /// Gets or sets the current flow in which the current thread is running.
        /// </summary>
        public/*internal*/ static Flow CurrentFlow
        {
            get { return currentFlow; }
            set { currentFlow = value; }
        }

        /// <summary>
        /// Gets or sets the default exception handler for all flows.
        /// </summary>
        public static Action<string, Exception> DefaultExceptionHandler { get; set; }

        /// <summary>
        /// Gets or sets the exception handler for this flow.
        /// </summary>
        public Action<string, Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// Gets the name of this flow.
        /// </summary>
        public string Name { get { return name; } }

        public LogLevel SlowHandlerLogLevel { get; set; }
        public int SlowHandlerLogThreshold { get; set; }

        public LogLevel SlowScopeLogLevel { get; set; }
        public int SlowScopeLogThreshold { get; set; }

        public LogLevel LongQueueLogLevel { get; set; }
        public int LongQueueLogThreshold { get; set; }

        static Flow()
        {
            DefaultExceptionHandler = OnException;
        }

        protected Flow()
        {
            binder = new Binder();
            caseStack = new CaseStack();
            name = GetType().Name;

            ExceptionHandler = DefaultExceptionHandler;

            SlowHandlerLogLevel = Config.Flow.Logging.SlowHandler.LogLevel;
            SlowHandlerLogThreshold = Config.Flow.Logging.SlowHandler.Threshold;
            SlowScopeLogLevel = Config.Flow.Logging.SlowScope.LogLevel;
            SlowScopeLogThreshold = Config.Flow.Logging.SlowScope.Threshold;
            LongQueueLogLevel = Config.Flow.Logging.LongQueue.LogLevel;
            LongQueueLogThreshold = Config.Flow.Logging.LongQueue.Threshold;
        }

        public static Binder.Token Bind<T>(T e, Action<T> action)
            where T : Event
        {
            return currentFlow.Subscribe(e, action);
        }

        public static Binder.Token Bind<T>(
            T e, Action<T> action, Predicate<T> predicate)
            where T : Event
        {
            return currentFlow.Subscribe(e, action, predicate);
        }

        public static Binder.Token Bind<T>(T e, Func<Coroutine, T, IEnumerator> routine)
            where T : Event
        {
            return currentFlow.Subscribe(e, routine);
        }

        public static Binder.Token Bind<T>(
            T e, Func<Coroutine, T, IEnumerator> routine, Predicate<T> predicate)
            where T : Event
        {
            return currentFlow.Subscribe(e, routine, predicate);
        }

        public static void Bind(Binder.Token binderToken)
        {
            currentFlow.Subscribe(binderToken);
        }

        public static Binder.Token Unbind<T>(T e, Action<T> action)
            where T : Event
        {
            return currentFlow.Unsubscribe(e, action);
        }

        public static Binder.Token Unbind<T>(
            T e, Action<T> action, Predicate<T> predicate)
            where T : Event
        {
            return currentFlow.Unsubscribe(e, action, predicate);
        }

        public static Binder.Token Unbind<T>(T e, Func<Coroutine, T, IEnumerator> routine)
            where T : Event
        {
            return currentFlow.Unsubscribe(e, routine);
        }

        public static Binder.Token Unbind<T>(
            T e, Func<Coroutine, T, IEnumerator> routine, Predicate<T> predicate)
            where T : Event
        {
            return currentFlow.Unsubscribe(e, routine, predicate);
        }

        public static void Unbind(Binder.Token binderToken)
        {
            currentFlow.Unsubscribe(binderToken);
        }

        /// <summary>
        /// Default exception handler.
        /// </summary>
        private static void OnException(string message, Exception e)
        {
            throw new Exception(message, e);
        }

        public void Publish(Event e)
        {
            Hub.Post(e);
        }

        public Binder.Token Subscribe<T>(T e, Action<T> action)
            where T : Event
        {
            return binder.Bind(e, new MethodHandler<T>(action));
        }

        public Binder.Token Subscribe<T>(
            T e, Action<T> action, Predicate<T> predicate)
            where T : Event
        {
            return binder.Bind(e,
                new ConditionalMethodHandler<T>(action, predicate));
        }

        public Binder.Token Subscribe<T>(T e, Func<Coroutine, T, IEnumerator> routine)
            where T : Event
        {
            return binder.Bind(e, new CoroutineHandler<T>(routine));
        }

        public Binder.Token Subscribe<T>(
            T e, Func<Coroutine, T, IEnumerator> routine, Predicate<T> predicate)
            where T : Event
        {
            return binder.Bind(e,
                new ConditionalCoroutineHandler<T>(routine, predicate));
        }

        public void Subscribe(Binder.Token token)
        {
            binder.Bind(token);
        }

        public Binder.Token Unsubscribe<T>(T e, Action<T> handler)
            where T : Event
        {
            return binder.Unbind(e, new MethodHandler<T>(handler));
        }

        public Binder.Token Unsubscribe<T>(T e, Action<T> handler, Predicate<T> predicate)
            where T : Event
        {
            return binder.Unbind(e,
                new ConditionalMethodHandler<T>(handler, predicate));
        }

        public Binder.Token Unsubscribe<T>(T e, Func<Coroutine, T, IEnumerator> handler)
            where T : Event
        {
            return binder.Unbind(e, new CoroutineHandler<T>(handler));
        }

        public Binder.Token Unsubscribe<T>(
            T e, Func<Coroutine, T, IEnumerator> handler, Predicate<T> predicate)
            where T : Event
        {
            return binder.Unbind(e,
                new ConditionalCoroutineHandler<T>(handler, predicate));
        }

        public void Unsubscribe(Binder.Token token)
        {
            binder.Unbind(token);
        }

        internal void UnsubscribeInternal(Binder.Token token)
        {
            binder.UnbindInternal(token);
        }

        public abstract Flow Startup();
        public abstract void Shutdown();

        public Flow Attach()
        {
            Hub.Instance.Attach(this);
            return this;
        }

        public Flow Detach()
        {
            Hub.Instance.Detach(this);
            return this;
        }

        /// <summary>
        /// Adds the specified case to this flow.
        /// </summary>
        public Flow Add(ICase c)
        {
            caseStack.Add(c);
            return this;
        }

        /// <summary>
        /// Removes the specified case from this flow.
        /// </summary>
        public Flow Remove(ICase c)
        {
            caseStack.Remove(c);
            return this;
        }

        /// <summary>
        /// Makes this flow subscribe to the specified channel.
        /// </summary>
        public Flow SubscribeTo(string channel)
        {
            Hub.Instance.Subscribe(this, channel);
            return this;
        }

        /// <summary>
        /// Makes this flow unsubscribe from the specified channel.
        /// </summary>
        public Flow UnsubscribeFrom(string channel)
        {
            Hub.Instance.Unsubscribe(this, channel);
            return this;
        }

        public abstract void Feed(Event e);

        /// <summary>
        /// Increments the channel reference count by one and returns the result.
        /// </summary>
        internal int AddChannelRef()
        {
            return Interlocked.Increment(ref channelRefCount);
        }

        /// <summary>
        /// Decrements the channel reference count by one and returns the result.
        /// </summary>
        internal int RemoveChannelRef()
        {
            return Interlocked.Decrement(ref channelRefCount);
        }

        /// <summary>
        /// Resets the channel reference count as zero.
        /// </summary>
        internal int ResetChannelRef()
        {
            return Interlocked.Exchange(ref channelRefCount, 0);
        }

        /// <summary>
        /// Called internally to dspatch the specified event to registered handlers.
        /// </summary>
        protected void Dispatch(Event e)
        {
            // Safeguard for exclusive exception handling environments,
            // like Unity3D.
            if (handlerChain.Count != 0)
            {
                handlerChain.Clear();
            }

            int chainLength = binder.BuildHandlerChain(e, equivalent, handlerChain);
            if (chainLength == 0)
            {
                // unhandled event
                return;
            }

            Handler handler;
            for (int i = 0, count = handlerChain.Count; i < count; ++i)
            {
                handler = handlerChain[i];
                try
                {
                    // Now using DateTime.UtcNow, instead of slow Stopwatch
                    DateTime begin = DateTime.UtcNow;

                    handler.Invoke(e);

                    DateTime end = DateTime.UtcNow;
                    long totalMilliseconds = (long)(end - begin).TotalMilliseconds;
                    if (totalMilliseconds >= SlowHandlerLogThreshold)
                    {
#if NETSTANDARD1_4
                        var methodInfo = handler.Action.GetMethodInfo();
#else
                        var methodInfo = handler.Action.Method;
#endif
                        Log.Emit(SlowHandlerLogLevel,
                            "{0} slow handler {1:#,0}ms {2}.{3} on {4}",
                            Name, totalMilliseconds,
                            methodInfo.DeclaringType, methodInfo.Name, e);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHandler(
                        String.Format("{0} {1} {2}", Name, handler.ToString(), e.ToString()),
                        ex);
                }
            }

            handlerChain.Clear();
        }

        /// <summary>
        /// Overridden by subclasses to build a startup handler chain.
        /// </summary>
        protected virtual void Setup() { }

        /// <summary>
        /// Called internally when this flow starts up.
        /// </summary>
        protected void SetupInternal()
        {
            Subscribe(Hub.HeartbeatEvent, OnHeartbeatEvent);
            Subscribe(new FlowStart(), OnFlowStart);
            Subscribe(new FlowStop(), OnFlowStop);

            Setup();
        }

        /// <summary>
        /// Overridden by subclasses to build a shutdown handler chain.
        /// </summary>
        protected virtual void Teardown() { }

        /// <summary>
        /// Called internally when this flow shuts down.
        /// </summary>
        protected void TeardownInternal()
        {
            Teardown();

            Unsubscribe(new FlowStop(), OnFlowStop);
            Unsubscribe(new FlowStart(), OnFlowStart);
            Unsubscribe(Hub.HeartbeatEvent, OnHeartbeatEvent);
        }

        /// <summary>
        /// Overridden by subclasses to build a HeartbeatEvent handler chain.
        /// </summary>
        protected virtual void OnHeartbeat() { }

        /// <summary>
        /// Overridden by subclasses to build a FlowStart event handler chain.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// Overridden by subclasses to build a FlowStop event handler chain.
        /// </summary>
        protected virtual void OnStop() { }

        // HeartbeatEvent handler
        private void OnHeartbeatEvent(HeartbeatEvent e)
        {
            OnHeartbeat();
        }

        // FlowStart event handler
        private void OnFlowStart(FlowStart e)
        {
            OnStart();
        }

        // FlowStop event handler
        private void OnFlowStop(FlowStop e)
        {
            OnStop();
        }
    }
}
