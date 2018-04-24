// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

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
        public static Yield WaitForNothing(this Coroutine self)
        {
            return new WaitForNothing(self);
        }

        // WaitForSingleEvent/Response

        /// <summary>
        /// Waits for a single event until the default timeout.
        /// </summary>
        public static Yield WaitForSingleEvent(this Coroutine self, Event e)
        {
            return new WaitForSingleEvent(self, e);
        }

        /// <summary>
        /// Waits for a single event until the specified timeout in seconds.
        /// </summary>
        public static Yield WaitForSingleEvent(this Coroutine self,
            Event e, double seconds)
        {
            return new WaitForSingleEvent(self, e, seconds);
        }

        /// <summary>
        /// Posts the request and waits for a single response until default timeout.
        /// </summary>
        public static Yield WaitForSingleResponse(this Coroutine self,
            Event request, Event response)
        {
            return new WaitForSingleResponse(self, request, response);
        }

        /// <summary>
        /// Posts the request and waits for a single response until the specified
        /// timeout in seconds.
        /// </summary>
        public static Yield WaitForSingleResponse(this Coroutine self,
            Event request, Event response, double seconds)
        {
            return new WaitForSingleResponse(self, request, response, seconds);
        }

        // WaitForMultipleEvents/Responses

        /// <summary>
        /// Waits for multiple events until the default timeout.
        /// </summary>
        public static Yield WaitForMultipleEvents(this Coroutine self,
            params Event[] e)
        {
            return new WaitForMultipleEvents(self, e);
        }

        /// <summary>
        /// Waits for multiple events until the specified timeout in seconds.
        /// </summary>
        public static Yield WaitForMultipleEvents(this Coroutine self,
            double seconds, params Event[] e)
        {
            return new WaitForMultipleEvents(self, seconds, e);
        }

        /// <summary>
        /// Posts the requests and waits for multiple responses until default
        /// timeout.
        /// </summary>
        public static Yield WaitForMultipleResponses(this Coroutine self,
            Event[] requests, params Event[] responses)
        {
            return new WaitForMultipleResponses(self, requests, responses);
        }

        /// <summary>
        /// Posts the requests and waits for multiple responses until the
        /// specified timeout in seconds.
        /// </summary>
        public static Yield WaitForMultipleResponse(this Coroutine self,
            Event[] requests, double seconds, params Event[] responses)
        {
            return new WaitForMultipleResponses(self, requests, seconds, responses);
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

    }
}
