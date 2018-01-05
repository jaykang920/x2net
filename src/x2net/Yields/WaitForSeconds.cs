// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for the specified time in seconds.
    /// </summary>
    public class WaitForSeconds : Yield
    {
        private readonly Coroutine coroutine;
        private readonly Binder.Token token;

        public WaitForSeconds(Coroutine coroutine, double seconds)
        {
            this.coroutine = coroutine;
            TimeoutEvent e = new TimeoutEvent { Key = this };
            token = Flow.Bind(e, OnTimeout);
            TimeFlow.Default.Reserve(e, seconds);
        }

        void OnTimeout(TimeoutEvent e)
        {
            Flow.Unbind(token);

            coroutine.Result = e;
            coroutine.Continue();
        }
    }

    public class WaitForNothing : Yield
    {
        private readonly Coroutine coroutine;
        private readonly object result;
        private readonly Binder.Token token;

        public WaitForNothing(Coroutine coroutine) : this(coroutine, null)
        {
        }

        public WaitForNothing(Coroutine coroutine, object result)
        {
            this.coroutine = coroutine;
            this.result = result;
            TimeoutEvent e = new TimeoutEvent { Key = this };
            token = Flow.Bind(e, OnTimeout);
            Hub.Post(e);
        }

        void OnTimeout(TimeoutEvent e)
        {
            Flow.Unbind(token);

            coroutine.Result = result;
            coroutine.Continue();
        }
    }
}
