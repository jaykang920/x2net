﻿// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for a single event.
    /// </summary>
    public class WaitForEvent : Yield
    {
        private readonly Binding.Token handlerToken;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token? timerToken;

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
        }

        void OnEvent(Event e)
        {
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

            coroutine.Result = e;
            coroutine.Continue();
        }

        void OnTimeout(TimeoutEvent e)
        {
            Flow.Unbind(handlerToken);
            Flow.Unbind(timeoutToken);

            int waitHandle = handlerToken.Key._WaitHandle;
            if (waitHandle != 0)
            {
                WaitHandlePool.Release(waitHandle);
            }

            Trace.Error("WaitForEvent timeout for {0}", handlerToken.Key);

            coroutine.Status = CoroutineStatus.Timeout;
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
