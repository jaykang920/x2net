// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for the completion of another coroutine.
    /// </summary>
    public class WaitForCompletion : Yield
    {
        public WaitForCompletion(Coroutine coroutine,
            Func<Coroutine, IEnumerator> func)
        {
            Coroutine c = new Coroutine(coroutine);
            c.Start(func(c));
        }
    }

    /// <summary>
    /// YieldInstruction that waits for the completion of another coroutine with
    /// a single additional argument.
    /// </summary>
    public class WaitForCompletion<T> : Yield
    {
        public WaitForCompletion(Coroutine coroutine,
            Func<Coroutine, T, IEnumerator> func, T arg)
        {
            Coroutine c = new Coroutine(coroutine);
            c.Start(func(c, arg));
        }
    }

    /// <summary>
    /// YieldInstruction that waits for the completion of another coroutine with
    /// two additional arguments.
    /// </summary>
    public class WaitForCompletion<T1, T2> : Yield
    {
        public WaitForCompletion(Coroutine coroutine,
            Func<Coroutine, T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
        {
            Coroutine c = new Coroutine(coroutine);
            c.Start(func(c, arg1, arg2));
        }
    }

    /// <summary>
    /// YieldInstruction that waits for the completion of another coroutine with
    /// three additional arguments.
    /// </summary>
    public class WaitForCompletion<T1, T2, T3> : Yield
    {
        public WaitForCompletion(Coroutine coroutine,
            Func<Coroutine, T1, T2, T3, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3)
        {
            Coroutine c = new Coroutine(coroutine);
            c.Start(func(c, arg1, arg2, arg3));
        }
    }
}
