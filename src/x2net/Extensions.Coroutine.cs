// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;
#if NET40
using System.Threading.Tasks;
#endif

namespace x2net
{
    // Extensions.Coroutine
    // Inline coroutine activation methods (used with 'yield return' statements)
    public static partial class Extensions
    {
        // WaitForSeconds

        /// <summary>
        /// Waits for the specified time in seconds.
        /// </summary>
        public static Yield WaitForSeconds(this Coroutine self, double seconds)
        {
            return new WaitForSeconds(self, seconds);
        }

        /// <summary>
        /// Waits untill the next handler execution chance.
        /// </summary>
        public static Yield WaitForNext(this Coroutine self)
        {
            return new WaitForNext(self);
        }

        // WaitForEvent/Response

        /// <summary>
        /// Waits for a single event until the default timeout.
        /// </summary>
        public static Yield WaitForEvent(this Coroutine self, Event e)
        {
            return new WaitForEvent(self, e);
        }

        /// <summary>
        /// Waits for a single event until the specified timeout in seconds.
        /// </summary>
        public static Yield WaitForEvent(this Coroutine self,
            Event e, double seconds)
        {
            return new WaitForEvent(self, e, seconds);
        }

        /// <summary>
        /// Posts the request and waits for a single response until default timeout.
        /// </summary>
        public static Yield WaitForResponse(this Coroutine self,
            Event request, Event response)
        {
            return new WaitForResponse(self, request, response);
        }

        /// <summary>
        /// Posts the request and waits for a single response until the specified
        /// timeout in seconds.
        /// </summary>
        public static Yield WaitForResponse(this Coroutine self,
            Event request, Event response, double seconds)
        {
            return new WaitForResponse(self, request, response, seconds);
        }

        // WaitForAllEvents/Responses

        /// <summary>
        /// Waits for all of multiple events until the default timeout.
        /// </summary>
        public static Yield WaitForAllEvents(this Coroutine self,
            params Event[] e)
        {
            return new WaitForAllEvents(self, e);
        }

        /// <summary>
        /// Waits for all of multiple events until the specified timeout in seconds.
        /// </summary>
        public static Yield WaitForAllEvents(this Coroutine self,
            double seconds, params Event[] e)
        {
            return new WaitForAllEvents(self, seconds, e);
        }

        /// <summary>
        /// Posts the requests and waits for all of multiple responses until default
        /// timeout.
        /// </summary>
        public static Yield WaitForAllResponses(this Coroutine self,
            Event[] requests, params Event[] responses)
        {
            return new WaitForAllResponses(self, requests, responses);
        }

        /// <summary>
        /// Posts the requests and waits for all of multiple responses until the
        /// specified timeout in seconds.
        /// </summary>
        public static Yield WaitForAllResponses(this Coroutine self,
            Event[] requests, double seconds, params Event[] responses)
        {
            return new WaitForAllResponses(self, requests, seconds, responses);
        }

        // WaitForAnyEvent/Response

        /// <summary>
        /// Waits for any of multiple events until the default timeout.
        /// </summary>
        public static Yield WaitForAnyEvent(this Coroutine self,
            params Event[] e)
        {
            return new WaitForAnyEvent(self, e);
        }

        /// <summary>
        /// Waits for any of multiple events until the specified timeout in seconds.
        /// </summary>
        public static Yield WaitForAnyEvent(this Coroutine self,
            double seconds, params Event[] e)
        {
            return new WaitForAnyEvent(self, seconds, e);
        }

        /// <summary>
        /// Posts the requests and waits for any of multiple responses until
        /// default timeout.
        /// </summary>
        public static Yield WaitForAnyResponse(this Coroutine self,
            Event[] requests, params Event[] responses)
        {
            return new WaitForAnyResponse(self, requests, responses);
        }

        /// <summary>
        /// Posts the requests and waits for any of multiple responses until the
        /// specified timeout in seconds.
        /// </summary>
        public static Yield WaitForAnyResponse(this Coroutine self,
            Event[] requests, double seconds, params Event[] responses)
        {
            return new WaitForAnyResponse(self, requests, seconds, responses);
        }

        // WaitForCopmpletion

        /// <summary>
        /// Waits for the completion of another coroutine.
        /// </summary>
        public static Yield WaitForCompletion(this Coroutine self,
            Func<Coroutine, IEnumerator> func)
        {
            return new WaitForCompletion(self, func);
        }

        /// <summary>
        /// Waits for the completion of another coroutine with a single
        /// additional argument.
        /// </summary>
        public static Yield WaitForCompletion<T>(this Coroutine self,
            Func<Coroutine, T, IEnumerator> func, T arg)
        {
            return new WaitForCompletion<T>(self, func, arg);
        }

        /// <summary>
        /// Waits for the completion of another coroutine with two additional
        /// arguments.
        /// </summary>
        public static Yield WaitForCompletion<T1, T2>(this Coroutine self,
            Func<Coroutine, T1, T2, IEnumerator> func, T1 arg1, T2 arg2)
        {
            return new WaitForCompletion<T1, T2>(self, func, arg1, arg2);
        }

        /// <summary>
        /// Waits for the completion of another coroutine with three additional
        /// arguments.
        /// </summary>
        public static Yield WaitForCompletion<T1, T2, T3>(this Coroutine self,
            Func<Coroutine, T1, T2, T3, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3)
        {
            return new WaitForCompletion<T1, T2, T3>(self, func, arg1, arg2, arg3);
        }

#if NET40
        /// <summary>
        /// Waits for the completion of another coroutine with three additional
        /// arguments.
        /// </summary>
        public static Yield WaitForCompletion<T1, T2, T3, T4>(this Coroutine self,
            Func<Coroutine, T1, T2, T3, T4, IEnumerator> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return new WaitForCompletion<T1, T2, T3, T4>(self, func, arg1, arg2, arg3, arg4);
        }
#endif

#if NET45
        /// <summary>
        /// Waits for a single task until the default timeout.
        /// </summary>
        public static Yield WaitForTask(this Coroutine self, Task task)
        {
            return new WaitForTask(self, task);
        }

        /// <summary>
        /// Waits for a single task until the specified timeout in seconds.
        /// </summary>
        public static Yield WaitForTask(this Coroutine self, Task task, double seconds)
        {
            return new WaitForTask(self, task, seconds);
        }

        /// <summary>
        /// Waits for a single task until the default timeout.
        /// </summary>
        public static Yield WaitForTask<T>(this Coroutine self, Task<T> task)
        {
            return new WaitForTask<T>(self, task);
        }

        /// <summary>
        /// Waits for a single task until the specified timeout in seconds.
        /// </summary>
        public static Yield WaitForTask<T>(this Coroutine self, Task<T> task, double seconds)
        {
            return new WaitForTask<T>(self, task, seconds);
        }
#endif
    }
}
