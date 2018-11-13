// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for the next execution frame.
    /// </summary>
    public class WaitForNext : Yield
    {
        private readonly object result;
        private readonly CoroutineStatus status;
        private readonly Binding.Token token;

        public WaitForNext(Coroutine coroutine) : this(coroutine, null, CoroutineStatus.Ok)
        {
        }

        public WaitForNext(Coroutine coroutine, object result, CoroutineStatus status)
            : base(coroutine)
        {
            this.result = result;
            this.status = status;
            TimeoutEvent e = new TimeoutEvent { Key = this };
            token = Flow.Bind(e, OnTimeout);
            Hub.Post(e);
        }

        void OnTimeout(TimeoutEvent e)
        {
            Flow.Unbind(token);

            coroutine.Result = result;
            coroutine.Status = status;
            coroutine.Continue();
        }
    }
}
