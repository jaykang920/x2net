// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for the specified time in seconds.
    /// </summary>
    public class WaitForSeconds : Yield
    {
        private readonly Binding.Token token;

        public WaitForSeconds(Coroutine coroutine, double seconds)
            : base(coroutine)
        {
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
}
