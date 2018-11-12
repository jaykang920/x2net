// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for any of multiple events.
    /// </summary>
    public class WaitForAnyEvent : Yield
    {
        private readonly Event[] expected;

        private readonly Binding.Token[] handlerTokens;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        private int waitHandle;

        public WaitForAnyEvent(Coroutine coroutine, params Event[] e)
            : this(coroutine, null, Config.Coroutine.DefaultTimeout, e)
        {
        }

        public WaitForAnyEvent(Coroutine coroutine, double seconds,
                params Event[] e)
            : this(coroutine, null, seconds, e)
        {
        }

        protected WaitForAnyEvent(Coroutine coroutine, Event[] requests,
            double seconds, params Event[] e)
            : base(coroutine)
        {
            if (!ReferenceEquals(requests, null))
            {
                waitHandle = WaitHandlePool.Acquire();
                for (int i = 0, count = requests.Length; i < count; ++i)
                {
                    requests[i]._WaitHandle = waitHandle;
                }
                for (int i = 0, count = e.Length; i < count; ++i)
                {
                    e[i]._WaitHandle = waitHandle;
                }
            }

            expected = e;

            handlerTokens = new Binding.Token[expected.Length];
            for (int i = 0; i < expected.Length; ++i)
            {
                handlerTokens[i] = Flow.Bind(expected[i], OnEvent);
            }

            if (seconds > 0)
            {
                TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
                timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
                timerToken = TimeFlow.Default.Reserve(timeoutEvent, seconds);
            }
        }

        void OnEvent(Event e)
        {
            int i;
            int length = expected.Length;
            for (i = 0; i < length; ++i)
            {
                if (expected[i].Equivalent(e))
                {
                    break;
                }
            }

            if (i >= length)
            {
                return;
            }

            for (i = 0; i < length; ++i)
            {
                Flow.Unbind(handlerTokens[i]);
            }

            if (timerToken.HasValue)
            {
                TimeFlow.Default.Cancel(timerToken.Value);
                Flow.Unbind(timeoutToken);
            }

            if (waitHandle != 0)
            {
                WaitHandlePool.Release(waitHandle);
            }

            coroutine.Result = e;
            coroutine.Continue();
        }

        void OnTimeout(TimeoutEvent e)
        {
            for (int i = 0, length = expected.Length; i < length; ++i)
            {
                Flow.Unbind(handlerTokens[i]);
            }

            Flow.Unbind(timeoutToken);

            if (waitHandle != 0)
            {
                WaitHandlePool.Release(waitHandle);
            }

            Trace.Error("WaitForAnyEvent timeout for {0}", expected);

            coroutine.Status = CoroutineStatus.Timeout;
            coroutine.Result = null;
            coroutine.Continue();
        }
    }

    /// <summary>
    /// YieldInstruction that posts requests and waits for multiple responses.
    /// </summary>
    public class WaitForAnyResponse : WaitForAnyEvent
    {
        public WaitForAnyResponse(Coroutine coroutine, Event[] requests,
                params Event[] responses)
            : this(coroutine, requests, Config.Coroutine.DefaultTimeout, responses)
        {
        }

        public WaitForAnyResponse(Coroutine coroutine, Event[] requests,
                double seconds, params Event[] responses)
            : base(coroutine, requests, seconds, responses)
        {
            if (ReferenceEquals(requests, null))
            {
                throw new ArgumentNullException();
            }
            for (int i = 0; i < requests.Length; ++i)
            {
                requests[i].Post();
            }
        }
    }
}
