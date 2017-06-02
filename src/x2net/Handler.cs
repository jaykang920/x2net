// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections;

namespace x2net
{
    /// <summary>
    /// Abstract base class for concrete event handlers.
    /// </summary>
    public abstract class Handler
    {
        /// <summary>
        /// Gets the underlying delegate of this handler.
        /// </summary>
        public abstract Delegate Action { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current
        /// object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Handler other = (Handler)obj;
            return Action.Equals(other.Action);
        }

        /// <summary>
        /// Returns the hash code for the current object.
        /// </summary>
        public override int GetHashCode()
        {
            return Action.GetHashCode();
        }

        /// <summary>
        /// Invokes the underlying delegate of this handler with the specified
        /// event.
        /// </summary>
        public abstract void Invoke(Event e);
    }

    /// <summary>
    /// Represents a generic method handler.
    /// </summary>
    public class MethodHandler<T> : Handler
        where T : Event
    {
        protected readonly Action<T> action;

        public override Delegate Action { get { return action; } }

        public MethodHandler(Action<T> action)
        {
            this.action = action;
        }

        public override void Invoke(Event e)
        {
            action((T)e);
        }
    }

    /// <summary>
    /// Represents a coroutine method handler.
    /// </summary>
    public class CoroutineHandler<T> : Handler
        where T : Event
    {
        protected readonly Func<Coroutine, T, IEnumerator> action;

        public override Delegate Action { get { return action; } }

        public CoroutineHandler(Func<Coroutine, T, IEnumerator> action)
        {
            this.action = action;
        }

        public override void Invoke(Event e)
        {
            Coroutine coroutine = new Coroutine();
            coroutine.Start(action(coroutine, (T)e));
        }
    }

    /// <summary>
    /// Represents a conditional generic method handler.
    /// </summary>
    public class ConditionalMethodHandler<T> : MethodHandler<T>
        where T : Event
    {
        private readonly Predicate<T> predicate;

        public ConditionalMethodHandler(Action<T> action, Predicate<T> predicate)
            : base(action)
        {
            this.predicate = predicate;
        }

        public override void Invoke(Event e)
        {
            if (predicate((T)e))
            {
                base.Invoke((T)e);
            }
        }
    }

    /// <summary>
    /// Represents a conditional coroutine method handler.
    /// </summary>
    public class ConditionalCoroutineHandler<T> : CoroutineHandler<T>
        where T : Event
    {
        private readonly Predicate<T> predicate;

        public ConditionalCoroutineHandler(Func<Coroutine, T, IEnumerator> action,
            Predicate<T> predicate) : base(action)
        {
            this.predicate = predicate;
        }

        public override void Invoke(Event e)
        {
            if (predicate((T)e))
            {
                base.Invoke((T)e);
            }
        }
    }
}
