// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for all of multiple events.
    /// </summary>
    public class WaitForAllEvents : Yield
    {
        private readonly Coroutine coroutine;
        private readonly Event[] expected, actual;

        private readonly Binder.Token[] handlerTokens;
        private readonly Binder.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        private int count;
        private int waitHandle;

        public WaitForAllEvents(Coroutine coroutine, params Event[] e)
            : this(coroutine, null, Config.Coroutine.DefaultTimeout, e)
        {
        }

        public WaitForAllEvents(Coroutine coroutine, double seconds,
                params Event[] e)
            : this(coroutine, null, seconds, e)
        {
        }

        protected WaitForAllEvents(Coroutine coroutine, Event[] requests,
            double seconds, params Event[] e)
        {
            this.coroutine = coroutine;

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
            actual = new Event[expected.Length];

            handlerTokens = new Binder.Token[expected.Length];
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
            for (int i = 0; i < expected.Length; ++i)
            {
                if (actual[i] == null && expected[i].Equivalent(e))
                {
                    Flow.Unbind(handlerTokens[i]);
                    handlerTokens[i] = new Binder.Token();
                    actual[i] = e;
                    ++count;
                    break;
                }
            }

            if (count >= expected.Length)
            {
                if (timerToken.HasValue)
                {
                    TimeFlow.Default.Cancel(timerToken.Value);
                    Flow.Unbind(timeoutToken);
                }

                if (waitHandle != 0)
                {
                    WaitHandlePool.Release(waitHandle);
                }

                coroutine.Result = actual;
                coroutine.Continue();
            }
        }

        void OnTimeout(TimeoutEvent e)
        {
            for (int i = 0, count = actual.Length; i < count; ++i)
            {
                if (ReferenceEquals(actual[i], null))
                {
                    Flow.Unbind(handlerTokens[i]);
                }
            }
            Flow.Unbind(timeoutToken);

            if (waitHandle != 0)
            {
                WaitHandlePool.Release(waitHandle);
            }

            Trace.Error("WaitForAllEvents timeout for {0}", expected);

            coroutine.Status = CoroutineStatus.Timeout;
            coroutine.Result = actual;  // incomplete array indicates timeout
            coroutine.Continue();
        }
    }

    /// <summary>
    /// YieldInstruction that posts requests and waits for multiple responses.
    /// </summary>
    public class WaitForAllResponses : WaitForAllEvents
    {
        public WaitForAllResponses(Coroutine coroutine, Event[] requests,
                params Event[] responses)
            : this(coroutine, requests, Config.Coroutine.DefaultTimeout, responses)
        {
        }

        public WaitForAllResponses(Coroutine coroutine, Event[] requests,
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
