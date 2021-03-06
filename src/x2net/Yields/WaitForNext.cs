﻿// Copyright (c) 2017-2019 Jae-jun Kang
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

        public WaitForNext(Coroutine coroutine, object result)
            : this(coroutine, result, CoroutineStatus.Ok)
        {
        }

        public WaitForNext(Coroutine coroutine, object result, CoroutineStatus status)
            : base(coroutine)
        {
            this.result = result;
            this.status = status;
            var e = new LocalEvent { Key = this };
            token = Flow.Bind(e, OnEvent);
            Hub.Post(e);
        }

        void OnEvent(LocalEvent e)
        {
            Flow.Unbind(token);

            coroutine.Result = result;
            coroutine.Status = status;
            coroutine.Continue();
        }
    }
}
