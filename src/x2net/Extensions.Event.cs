// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace x2net
{
    // Extensions.Event
    public static partial class Extensions
    {
        #region Aliases of Flow.Bind()

        /// <summary>
        /// Alias of Flow.Bind(T, Action(T)).
        /// </summary>
        public static T Bind<T>(this T self, Action<T> handler) where T : Event
        {
            Flow.Bind(self, handler);
            return self;
        }

        /// <summary>
        /// Alias of Flow.Bind(T, Action(T), Predicate(T)).
        /// </summary>
        public static T Bind<T>(this T self, Action<T> handler, Predicate<T> predicate)
            where T : Event
        {
            Flow.Bind(self, handler, predicate);
            return self;
        }

        /// <summary>
        /// Alias of Flow.Bind(T, Func(Coroutine, T, IEnumerator)).
        /// </summary>
        public static T Bind<T>(this T self, Func<Coroutine, T, IEnumerator> handler)
            where T : Event
        {
            Flow.Bind(self, handler);
            return self;
        }

        /// <summary>
        /// Alias of Flow.Bind(T, Func(Coroutine, T, IEnumerator), Predicate(T)).
        /// </summary>
        public static T Bind<T>(this T self, Func<Coroutine, T, IEnumerator> handler,
            Predicate<T> predicate) where T : Event
        {
            Flow.Bind(self, handler, predicate);
            return self;
        }

        #endregion

        /// <summary>
        /// Indicates that the event is the response of the specified one.
        /// </summary>
        public static T InResponseOf<T>(this T self, Event request)
            where T : Event
        {
            if (request._Handle != 0)
            {
                self._Handle = request._Handle;
            }
            if (request._WaitHandle != 0)
            {
                self._WaitHandle = request._WaitHandle;
            }
            return self;
        }

        /// <summary>
        /// Indicates that the event is associated with the specified hub
        /// channel.
        /// </summary>
        public static T InChannel<T>(this T self, string channel)
            where T : Event
        {
            self._Channel = channel;
            return self;
        }

        /// <summary>
        /// Alias of Hub.Post(e).
        /// </summary>
        public static void Post(this Event self)
        {
            Hub.Post(self);
        }

        #region Aliases of Flow.Unbind()

        /// <summary>
        /// Alias of Flow.Unbind(T, Action(T)).
        /// </summary>
        public static T Unbind<T>(this T self, Action<T> handler) where T : Event
        {
            Flow.Unbind(self, handler);
            return self;
        }

        /// <summary>
        /// Alias of Flow.Unbind(T, Action(T), Predicate(T)).
        /// </summary>
        public static T Unbind<T>(this T self, Action<T> handler, Predicate<T> predicate)
            where T : Event
        {
            Flow.Unbind(self, handler, predicate);
            return self;
        }

        /// <summary>
        /// Alias of Flow.Unbind(T, Func(Coroutine, T, IEnumerator)).
        /// </summary>
        public static T Unbind<T>(this T self, Func<Coroutine, T, IEnumerator> handler)
            where T : Event
        {
            Flow.Unbind(self, handler);
            return self;
        }

        /// <summary>
        /// Alias of Flow.Unbind(T, Func(Coroutine, T, IEnumerator), Predicate(T)).
        /// </summary>
        public static T Unbind<T>(this T self, Func<Coroutine, T, IEnumerator> handler,
            Predicate<T> predicate) where T : Event
        {
            Flow.Unbind(self, handler, predicate);
            return self;
        }

        #endregion
    }
}
