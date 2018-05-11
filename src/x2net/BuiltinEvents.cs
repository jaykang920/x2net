// auto-generated by x2net.xpiler

using System;
using System.Collections.Generic;
using System.Text;

using x2net;

namespace x2net
{
    /// <summary>
    /// Event type identifiers for built-in events.
    /// </summary>
    public static class BuiltinEventType
    {
        public const int HeartbeatEvent = -1;
        public const int FlowStart = -2;
        public const int FlowStop = -3;
        public const int TimeoutEvent = -4;
        public const int PeriodicEvent = -5;

        private static ConstsInfo<int> info;

        static BuiltinEventType()
        {
            info = new ConstsInfo<int>();
            info.Add("HeartbeatEvent", -1);
            info.Add("FlowStart", -2);
            info.Add("FlowStop", -3);
            info.Add("TimeoutEvent", -4);
            info.Add("PeriodicEvent", -5);
        }

        public static bool ContainsName(string name)
        {
            return info.ContainsName(name);
        }

        public static bool ContainsValue(int value)
        {
            return info.ContainsValue(value);
        }

        public static string GetName(int value)
        {
            return info.GetName(value);
        }

        public static int Parse(string name)
        {
            return info.Parse(name);
        }

        public static bool TryParse(string name, out int result)
        {
            return info.TryParse(name, out result);
        }
    }

    /// <summary>
    /// x2 subsystem heartbeat event.
    /// </summary>
    public class HeartbeatEvent : Event
    {
        protected static new readonly Tag tag;

        public static new int TypeId { get { return tag.TypeId; } }

        static HeartbeatEvent()
        {
            tag = new Tag(Event.tag, typeof(HeartbeatEvent), 0,
                    (int)BuiltinEventType.HeartbeatEvent);
        }

        public new static HeartbeatEvent New()
        {
            return new HeartbeatEvent();
        }

        public HeartbeatEvent()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected HeartbeatEvent(int length)
            : base(length + tag.NumProps)
        {
            Initialize();
        }

        protected override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            var hash = new Hash(base.GetHashCode(fingerprint));
            return hash.Code;
        }

        public override int GetTypeId()
        {
            return tag.TypeId;
        }

        public override Cell.Tag GetTypeTag() 
        {
            return tag;
        }

        public override Func<Event> GetFactoryMethod()
        {
            return HeartbeatEvent.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            return true;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
        }

        public override int GetLength(Type targetType, ref bool flag)
        {
            int length = base.GetLength(targetType, ref flag);
            if (!flag) { return length; }
            if (targetType != null && targetType == typeof(HeartbeatEvent))
            {
                flag = false;
            }
            return length;
        }

        public override void Serialize(Serializer serializer,
            Type targetType, ref bool flag)
        {
            base.Serialize(serializer, targetType, ref flag);
            if (!flag) { return; }
            if (targetType != null && targetType == typeof(HeartbeatEvent))
            {
                flag = false;
            }
        }

        public override void Serialize(VerboseSerializer serializer,
            Type targetType, ref bool flag)
        {
            base.Serialize(serializer, targetType, ref flag);
            if (!flag) { return; }
            if (targetType != null && targetType == typeof(HeartbeatEvent))
            {
                flag = false;
            }
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
        }

        private void Initialize()
        {
        }
    }

    /// <summary>
    /// A local event enqueued when a flow starts.
    /// </summary>
    public class FlowStart : Event
    {
        protected static new readonly Tag tag;

        public static new int TypeId { get { return tag.TypeId; } }

        static FlowStart()
        {
            tag = new Tag(Event.tag, typeof(FlowStart), 0,
                    (int)BuiltinEventType.FlowStart);
        }

        public new static FlowStart New()
        {
            return new FlowStart();
        }

        public FlowStart()
            : base(tag.NumProps)
        {
        }

        protected FlowStart(int length)
            : base(length + tag.NumProps)
        {
        }

        protected override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            var hash = new Hash(base.GetHashCode(fingerprint));
            return hash.Code;
        }

        public override int GetTypeId()
        {
            return tag.TypeId;
        }

        public override Cell.Tag GetTypeTag() 
        {
            return tag;
        }

        public override Func<Event> GetFactoryMethod()
        {
            return FlowStart.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            return true;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
        }
    }

    /// <summary>
    /// A local event enqueued when a flow stops.
    /// </summary>
    public class FlowStop : Event
    {
        protected static new readonly Tag tag;

        public static new int TypeId { get { return tag.TypeId; } }

        static FlowStop()
        {
            tag = new Tag(Event.tag, typeof(FlowStop), 0,
                    (int)BuiltinEventType.FlowStop);
        }

        public new static FlowStop New()
        {
            return new FlowStop();
        }

        public FlowStop()
            : base(tag.NumProps)
        {
        }

        protected FlowStop(int length)
            : base(length + tag.NumProps)
        {
        }

        protected override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            var hash = new Hash(base.GetHashCode(fingerprint));
            return hash.Code;
        }

        public override int GetTypeId()
        {
            return tag.TypeId;
        }

        public override Cell.Tag GetTypeTag() 
        {
            return tag;
        }

        public override Func<Event> GetFactoryMethod()
        {
            return FlowStop.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            return true;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
        }
    }

    /// <summary>
    /// A local timeout event.
    /// </summary>
    public class TimeoutEvent : Event
    {
        protected static new readonly Tag tag;

        public static new int TypeId { get { return tag.TypeId; } }

        private object key_;
        private int intParam_;

        /// <summary>
        /// Event key object.
        /// </summary>
        public object Key
        {
            get { return key_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                key_ = value;
            }
        }

        /// <summary>
        /// Optional integer parameter
        /// </summary>
        public int IntParam
        {
            get { return intParam_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                intParam_ = value;
            }
        }

        static TimeoutEvent()
        {
            tag = new Tag(Event.tag, typeof(TimeoutEvent), 2,
                    (int)BuiltinEventType.TimeoutEvent);
        }

        public new static TimeoutEvent New()
        {
            return new TimeoutEvent();
        }

        public TimeoutEvent()
            : base(tag.NumProps)
        {
        }

        protected TimeoutEvent(int length)
            : base(length + tag.NumProps)
        {
        }

        protected override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            TimeoutEvent o = (TimeoutEvent)other;
            if (key_ != o.key_)
            {
                return false;
            }
            if (intParam_ != o.intParam_)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            var hash = new Hash(base.GetHashCode(fingerprint));
            if (fingerprint.Length <= tag.Offset)
            {
                return hash.Code;
            }
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                hash.Update(tag.Offset + 0);
                hash.Update(key_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(intParam_);
            }
            return hash.Code;
        }

        public override int GetTypeId()
        {
            return tag.TypeId;
        }

        public override Cell.Tag GetTypeTag() 
        {
            return tag;
        }

        public override Func<Event> GetFactoryMethod()
        {
            return TimeoutEvent.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            TimeoutEvent o = (TimeoutEvent)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (key_ != o.key_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (intParam_ != o.intParam_)
                {
                    return false;
                }
            }
            return true;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Key:{0}", key_);
            stringBuilder.AppendFormat(" IntParam:{0}", intParam_);
        }
    }

    /// <summary>
    /// A local periodically recurring event.
    /// </summary>
    public class PeriodicEvent : Event
    {
        protected static new readonly Tag tag;

        public static new int TypeId { get { return tag.TypeId; } }

        private object key_;
        private int intParam_;

        /// <summary>
        /// Event key object.
        /// </summary>
        public object Key
        {
            get { return key_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                key_ = value;
            }
        }

        /// <summary>
        /// Optional integer parameter
        /// </summary>
        public int IntParam
        {
            get { return intParam_; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                intParam_ = value;
            }
        }

        static PeriodicEvent()
        {
            tag = new Tag(Event.tag, typeof(PeriodicEvent), 2,
                    (int)BuiltinEventType.PeriodicEvent);
        }

        public new static PeriodicEvent New()
        {
            return new PeriodicEvent();
        }

        public PeriodicEvent()
            : base(tag.NumProps)
        {
        }

        protected PeriodicEvent(int length)
            : base(length + tag.NumProps)
        {
        }

        protected override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }
            PeriodicEvent o = (PeriodicEvent)other;
            if (key_ != o.key_)
            {
                return false;
            }
            if (intParam_ != o.intParam_)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            var hash = new Hash(base.GetHashCode(fingerprint));
            if (fingerprint.Length <= tag.Offset)
            {
                return hash.Code;
            }
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                hash.Update(tag.Offset + 0);
                hash.Update(key_);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(intParam_);
            }
            return hash.Code;
        }

        public override int GetTypeId()
        {
            return tag.TypeId;
        }

        public override Cell.Tag GetTypeTag() 
        {
            return tag;
        }

        public override Func<Event> GetFactoryMethod()
        {
            return PeriodicEvent.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            PeriodicEvent o = (PeriodicEvent)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (key_ != o.key_)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (intParam_ != o.intParam_)
                {
                    return false;
                }
            }
            return true;
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Key:{0}", key_);
            stringBuilder.AppendFormat(" IntParam:{0}", intParam_);
        }
    }
}
