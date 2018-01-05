// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Text;

namespace x2net
{
    /// <summary>
    /// Common base class for all events.
    /// </summary>
    public class Event : Cell
    {
        /// <summary>
        /// Per-class type tag to support custom type hierarchy.
        /// </summary>
        protected new static readonly Tag tag;

        public static int TypeId { get { return tag.TypeId; } }

        private string _channel;
        private int _handle;
        private bool _transform = true;
        private int _waitHandle;

        /// <summary>
        /// Gets or sets the name of the hub channel which this event is
        /// assigned to.
        /// </summary>
        public string _Channel
        {
            get { return _channel; }
            set { _channel = value; }
        }

        /// <summary>
        /// Gets or sets the link session handle associated with this event.
        /// </summary>
        public int _Handle
        {
            get { return _handle; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                _handle = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether this event is to be
        /// transformed or not when it is transferred through a link.
        /// </summary>
        public bool _Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        /// <summary>
        /// Gets or sets the coroutine wait handle associated with this event.
        /// </summary>
        public int _WaitHandle
        {
            get { return _waitHandle; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                _waitHandle = value;
            }
        }

        static Event()
        {
            tag = new Tag(null, typeof(Event), 2, 0);
        }

        /// <summary>
        /// Initializes a new instance of the Event class.
        /// </summary>
        public Event() : base(tag.NumProps) { }

        /// <summary>
        /// Initializes a new Event instance with the given fingerprint length.
        /// </summary>
        protected Event(int length) : base(length + tag.NumProps) { }

        /// <summary>
        /// Creates a new instance of the Event class.
        /// </summary>
        public static Event New()
        {
            return new Event();
        }

        /// <summary>
        /// Overridden by subclasses to build a ToString chain.
        /// </summary>
        protected override void Describe(StringBuilder stringBuilder)
        {
            stringBuilder
                .Append(' ')
                .Append(GetTypeId());
        }

        /// <summary>
        /// Overridden by subclasses to build an equality test chain.
        /// </summary>
        protected override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            Event o = (Event)other;
            if (_handle != o._handle)
            {
                return false;
            }
            if (_waitHandle != o._waitHandle)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the hash code for the current object.
        /// </summary>
        public override int GetHashCode()
        {
            return GetHashCode(fingerprint, GetTypeId());
        }

        /// <summary>
        /// Returns the hash code for this event based on the specified
        /// fingerprint, assuming the given type identifier.
        /// </summary>
        public int GetHashCode(Fingerprint fingerprint, int typeId)
        {
            Hash hash = new Hash(GetHashCode(fingerprint));
            hash.Update(-1);  // separator
            hash.Update(typeId);
            return hash.Code;
        }

        /// <summary>
        /// Overridden by subclasses to build a hash code generator chain.
        /// </summary>
        public override int GetHashCode(Fingerprint fingerprint)
        {
            Hash hash = new Hash(base.GetHashCode(fingerprint));
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                hash.Update(tag.Offset + 0);
                hash.Update(_handle);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(_waitHandle);
            }
            return hash.Code;
        }

        /// <summary>
        /// Returns the type identifier of this event.
        /// </summary>
        /// <returns></returns>
        public virtual int GetTypeId()
        {
            return tag.TypeId;
        }

        /// <summary>
        /// Returns the custom type tag of this event.
        /// </summary>
        public override Cell.Tag GetTypeTag()
        {
            return tag;
        }

        /// <summary>
        /// Returns the factory method delegate that can create an instance of
        /// this event.
        /// </summary>
        public virtual Func<Event> GetFactoryMethod()
        {
            return Event.New;
        }

        /// <summary>
        /// Overridden by subclasses to build an equivalence test chain.
        /// </summary>
        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            Event o = (Event)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (_handle != o._handle)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (_waitHandle != o._waitHandle)
                {
                    return false;
                }
            }
            return true;
        }

        #region Serialization

        /// <summary>
        /// Overridden by subclasses to build a deserialization chain.
        /// </summary>
        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[1])
            {
                deserializer.Read(out _waitHandle);
            }
        }

        /// <summary>
        /// Overridden by subclasses to build a verbose deserialization chain.
        /// </summary>
        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("_WaitHandle", out _waitHandle);
        }

        /// <summary>
        /// Overridden by subclasses to build an encoded length computation chain.
        /// </summary>
        public override int GetLength(Type targetType, ref bool flag)
        {
            int length = Serializer.GetLength(GetTypeId());
            length += base.GetLength(targetType, ref flag);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[1])
            {
                length += Serializer.GetLength(_waitHandle);
            }
            if (targetType != null && targetType == typeof(Event))
            {
                flag = false;
            }
            return length;
        }

        /// <summary>
        /// Overridden by subclasses to build a serialization chain.
        /// </summary>
        public override void Serialize(Serializer serializer,
            Type targetType, ref bool flag)
        {
            base.Serialize(serializer, targetType, ref flag);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[1])
            {
                serializer.Write(_waitHandle);
            }
            if (targetType != null && targetType == typeof(Event))
            {
                flag = false;
            }
        }

        /// <summary>
        /// Overridden by subclasses to build a verbose serialization chain.
        /// </summary>
        public override void Serialize(VerboseSerializer serializer,
            Type targetType, ref bool flag)
        {
            base.Serialize(serializer, targetType, ref flag);
            serializer.Write("_WaitHandle", _waitHandle);
            if (targetType != null && targetType == typeof(Event))
            {
                flag = false;
            }
        }

        #endregion  // Serialization

        /// <summary>
        /// Supports light-weight custom type hierarchy for Event and its subclasses.
        /// </summary>
        public new class Tag : Cell.Tag
        {
            /// <summary>
            /// Gets the type identifier of this event type.
            /// </summary>
            public int TypeId { get; private set; }

            /// <summary>
            /// Initializes a new instance of the Event.Tag class.
            /// </summary>
            public Tag(Tag baseTag, Type runtimeType, int numProps, int typeId)
                : base(baseTag, runtimeType, numProps)
            {
                TypeId = typeId;
            }
        }
    }

    /// <summary>
    /// An event proxy to support hash table matching based on equivalence.
    /// </summary>
    public class EventEquivalent : Event
    {
        public Event InnerEvent { get; set; }
        public int InnerTypeId { get; set; }

        protected override bool EqualsTo(Cell other)
        {
            return other.Equivalent(InnerEvent, fingerprint);
        }

        public override int GetHashCode()
        {
            return InnerEvent.GetHashCode(fingerprint, InnerTypeId);
        }
    }
}
