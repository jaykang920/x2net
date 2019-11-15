// auto-generated by x2net.xpiler

using System;
using System.Collections.Generic;
using System.Text;

using x2net;

namespace hello
{
    public class HelloReq : Event
    {
        protected static new readonly Tag tag;

        public static new int TypeId { get { return tag.TypeId; } }

        private string name_;

        public string Name
        {
            get { return name_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                name_ = value;
            }
        }

        static HelloReq()
        {
            tag = new Tag(Event.tag, typeof(HelloReq), 1,
                    1);
        }

        public new static HelloReq New()
        {
            return new HelloReq();
        }

        public HelloReq()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected HelloReq(int length)
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
            HelloReq o = (HelloReq)other;
            if (name_ != o.name_)
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
                hash.Update(name_);
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
            return HelloReq.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            HelloReq o = (HelloReq)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (name_ != o.name_)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                deserializer.Read(out name_);
            }
        }

        public override int GetLength(Type targetType, ref bool flag)
        {
            int length = base.GetLength(targetType, ref flag);
            if (!flag) { return length; }
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetLength(name_);
            }
            if (targetType != null && targetType == typeof(HelloReq))
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
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(name_);
            }
            if (targetType != null && targetType == typeof(HelloReq))
            {
                flag = false;
            }
        }

        public bool HasName()
        {
            return fingerprint.Get(tag.Offset + 0);
        }

        public void Update(HelloReq o)
        {
            if (o.HasName()) { Name = o.Name; }
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Name:{0}", name_.ToStringEx());
        }

        private void Initialize()
        {
            name_ = "";
        }
    }

    public class HelloResp : Event
    {
        protected static new readonly Tag tag;

        public static new int TypeId { get { return tag.TypeId; } }

        private string message_;

        public string Message
        {
            get { return message_; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                message_ = value;
            }
        }

        static HelloResp()
        {
            tag = new Tag(Event.tag, typeof(HelloResp), 1,
                    2);
        }

        public new static HelloResp New()
        {
            return new HelloResp();
        }

        public HelloResp()
            : base(tag.NumProps)
        {
            Initialize();
        }

        protected HelloResp(int length)
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
            HelloResp o = (HelloResp)other;
            if (message_ != o.message_)
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
                hash.Update(message_);
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
            return HelloResp.New;
        }

        protected override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            HelloResp o = (HelloResp)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (message_ != o.message_)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                deserializer.Read(out message_);
            }
        }

        public override int GetLength(Type targetType, ref bool flag)
        {
            int length = base.GetLength(targetType, ref flag);
            if (!flag) { return length; }
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                length += Serializer.GetLength(message_);
            }
            if (targetType != null && targetType == typeof(HelloResp))
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
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                serializer.Write(message_);
            }
            if (targetType != null && targetType == typeof(HelloResp))
            {
                flag = false;
            }
        }

        public bool HasMessage()
        {
            return fingerprint.Get(tag.Offset + 0);
        }

        public void Update(HelloResp o)
        {
            if (o.HasMessage()) { Message = o.Message; }
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            base.Describe(stringBuilder);
            stringBuilder.AppendFormat(" Message:{0}", message_.ToStringEx());
        }

        private void Initialize()
        {
            message_ = "";
        }
    }
}
