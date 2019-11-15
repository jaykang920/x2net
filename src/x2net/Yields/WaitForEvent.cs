// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for a single event.
    /// </summary>
    public class WaitForEvent : Yield
    {
        [ThreadStatic]
        private static HashSet<WaitForEvent> waiting;

        private readonly Binding.Token handlerToken;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        private bool errorSuppressed;
        private Event request;

        public WaitForEvent(Coroutine coroutine, Event e)
            : this(coroutine, null, e, Config.Coroutine.DefaultTimeout)
        {
        }

        public WaitForEvent(Coroutine coroutine, Event e, double seconds)
            : this(coroutine, null, e, seconds)
        {
        }

        protected WaitForEvent(Coroutine coroutine, Event request, Event e, double seconds)
            : base(coroutine)
        {
            if (!ReferenceEquals(request, null))
            {
                this.request = request;

                int waitHandle = WaitHandlePool.Acquire();
                request._WaitHandle = waitHandle;
                e._WaitHandle = waitHandle;
            }

            handlerToken = Flow.Bind(e, OnEvent);
            // No timeout when seconds <= 0
            if (seconds > 0)
            {
                TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
                timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
                timerToken = TimeFlow.Instance.Reserve(timeoutEvent, seconds);
            }

            if (ReferenceEquals(waiting, null))
            {
                waiting = new HashSet<WaitForEvent>();
            }
            waiting.Add(this);
        }

        /// <summary>
        /// Immediately expires all the WaitForEvent instances that are wating
        /// for a finite timeout.
        /// </summary>
        public static void ExpireAll()
        {
            if (ReferenceEquals(waiting, null))
            {
                return;
            }

            foreach (var instance in waiting)
            {
                new TimeoutEvent { Key = instance, IntParam = 1 }.Post();
            }
        }

        /// <summary>
        /// Suppresses x2 trace error logging on timeout.
        /// </summary>
        public WaitForEvent SuppressError(bool flag)
        {
            errorSuppressed = flag;
            return this;
        }

        void OnEvent(Event e)
        {
            waiting.Remove(this);

            Flow.Unbind(handlerToken);

            if (timerToken.HasValue)
            {
                TimeFlow.Instance.Cancel(timerToken.Value);
                Flow.Unbind(timeoutToken);
            }

            int waitHandle = handlerToken.Key._WaitHandle;
            if (waitHandle != 0)
            {
                WaitHandlePool.Release(waitHandle);
            }

            request = null;

            coroutine.Result = e;
            coroutine.Continue();
        }

        void OnTimeout(TimeoutEvent e)
        {
            waiting.Remove(this);

            Flow.Unbind(handlerToken);

            if (e.IntParam != 0)
            {
                TimeFlow.Instance.Cancel(timerToken.Value);
            }
            Flow.Unbind(timeoutToken);

            int waitHandle = handlerToken.Key._WaitHandle;
            if (waitHandle != 0)
            {
                WaitHandlePool.Release(waitHandle);
            }

            if (!errorSuppressed)
            {
                string message = null;
                var traceLevel = (e.IntParam == 0 ? TraceLevel.Error : TraceLevel.Warning);
                var action = (e.IntParam == 0 ? "timeout" : "expired");
                var typeName = handlerToken.Key.GetTypeTag().RuntimeType.FullName;
                if (ReferenceEquals(request, null))
                {
                    message = string.Format("WaitForEvent {0} for {1}",
                        action, typeName);
                }
                else
                {
                    message = string.Format("WaitForResponse {0} for {1} with request {2}",
                        action, typeName, request);
                }
                Trace.Emit(traceLevel, message);
            }

            request = null;

            coroutine.Status = (e.IntParam == 0 ?
                CoroutineStatus.Timeout : CoroutineStatus.Canceled);
            coroutine.Result = null;  // indicates timeout
            coroutine.Continue();
        }
    }

    /// <summary>
    /// YieldInstruction that posts a request and waits for a single response.
    /// </summary>
    public class WaitForResponse : WaitForEvent
    {
        public WaitForResponse(Coroutine coroutine, Event request,
                Event response)
            : this(coroutine, request, response, Config.Coroutine.DefaultTimeout)
        {
        }

        public WaitForResponse(Coroutine coroutine, Event request,
                Event response, double seconds)
            : base(coroutine, request, response, seconds)
        {
            if (ReferenceEquals(request, null))
            {
                throw new ArgumentNullException();
            }
            request.Post();
        }
    }
}
