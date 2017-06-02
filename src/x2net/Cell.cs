// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Text;

namespace x2net
{
    /// <summary>
    /// Common base class for all custom types.
    /// </summary>
    public abstract class Cell
    {
        /// <summary>
        /// Per-class type tag to support custom type hierarchy.
        /// </summary>
        protected static readonly Tag tag;

        /// <summary>
        /// Fingerprint to keep track of property assignment status.
        /// </summary>
        protected Fingerprint fingerprint;

        static Cell()
        {
            tag = new Tag(null, typeof(Cell), 0);
        }

        /// <summary>
        /// Initializes a new Cell instance with the given fingerprint length.
        /// </summary>
        protected Cell(int length)
        {
            fingerprint = new Fingerprint(length);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this one.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            Cell other = obj as Cell;
            if ((object)other == null)
            {
                return false;
            }
            return other.EqualsTo(this);
        }

        /// <summary>
        /// Overridden by subclasses to build an equality test chain.
        /// </summary>
        protected virtual bool EqualsTo(Cell other)
        {
            if (GetType() != other.GetType())
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified Cell object is equivalent to this
        /// one.
        /// </summary>
        /// A Cell is said to be equivalent to the other if its fingerprint is
        /// equivalent to the other's, and all the fingerprinted properties of
        /// the other exactly matches with their counterparts.
        /// <remarks>
        /// Given two Cell objects x and y, x.Equivalent(y) returns true if:
        ///   <list type="bullet">
        ///     <item>x.fingerprint.Equivalent(y.fingerprint) returns true.
        ///     </item>
        ///     <item>All the fingerprinted properties in x are equal to those
        ///     in y.</item>
        ///   </list>
        /// </remarks>
        public bool Equivalent(Cell other)
        {
            return Equivalent(other, fingerprint);
        }

        /// <summary>
        /// Determines whether the specified Cell object is equivalent to this
        /// one based on the given fingerprint.
        /// </summary>
        public bool Equivalent(Cell other, Fingerprint fingerprint)
        {
            if (!other.IsKindOf(this))
            {
                return false;
            }
            if (!fingerprint.Equivalent(other.fingerprint))
            {
                return false;
            }
            return IsEquivalent(other, fingerprint);
        }

        /// <summary>
        /// Gets the fingerprint of this cell.
        /// </summary>
        public Fingerprint GetFingerprint()
        {
            return fingerprint;
        }

        /// <summary>
        /// Returns the hash code for the current object.
        /// </summary>
        public override int GetHashCode()
        {
            return GetHashCode(fingerprint);
        }

        /// <summary>
        /// Overridden by subclasses to build a hash code generator chain.
        /// </summary>
        public virtual int GetHashCode(Fingerprint fingerprint)
        {
            return Hash.Seed;
        }

        /// <summary>
        /// Returns the custom type tag of this cell.
        /// </summary>
        public virtual Tag GetTypeTag()
        {
            return tag;
        }

        /// <summary>
        /// Overridden by subclasses to build an equivalence test chain.
        /// </summary>
        protected virtual bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            return true;
        }

        /// <summary>
        /// Determines whether this Cell object is a kind of the specified Cell
        /// in the custom type hierarchy.
        /// </summary>
        public bool IsKindOf(Cell other)
        {
            Tag tag = GetTypeTag();
            Tag otherTag = other.GetTypeTag();
            while (tag != null)
            {
                if (tag == otherTag)
                {
                    return true;
                }
                tag = tag.Base;
            }
            return false;
        }

        /// <summary>
        /// Sets the fingerprint of this cell as the specified one.
        /// </summary>
        internal void SetFingerprint(Fingerprint fingerprint)
        {
            this.fingerprint = fingerprint;
        }

        /// <summary>
        /// Returns a string that describes the current object.
        /// </summary>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder
                .Append(GetTypeTag().RuntimeType.Name)
                .Append(" {");
            Describe(stringBuilder);
            stringBuilder.Append(" }");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Overridden by subclasses to build a ToString chain.
        /// </summary>
        protected virtual void Describe(StringBuilder stringBuilder)
        {
        }

        #region Serialization

        /// <summary>
        /// Overridden by subclasses to build a deserialization chain.
        /// </summary>
        public virtual void Deserialize(Deserializer deserializer)
        {
            fingerprint.Deserialize(deserializer);
        }

        /// <summary>
        /// Overridden by subclasses to build a verbose deserialization chain.
        /// </summary>
        public virtual void Deserialize(VerboseDeserializer deserializer)
        {
        }

        public int GetLength()
        {
            bool flag = true;
            return GetLength(null, ref flag);
        }

        /// <summary>
        /// Overridden by subclasses to build an encoded length computation chain.
        /// </summary>
        public virtual int GetLength(Type targetType, ref bool flag)
        {
            return fingerprint.GetLength();
        }

        public void Serialize(Serializer serializer)
        {
            bool flag = true;
            Serialize(serializer, null, ref flag);
        }

        /// <summary>
        /// Overridden by subclasses to build a serialization chain.
        /// </summary>
        public virtual void Serialize(Serializer serializer,
            Type targetType, ref bool flag)
        {
            fingerprint.Serialize(serializer);
        }

        public void Serialize(VerboseSerializer serializer)
        {
            bool flag = true;
            Serialize(serializer, null, ref flag);
        }

        /// <summary>
        /// Overridden by subclasses to build a verbose serialization chain.
        /// </summary>
        public virtual void Serialize(VerboseSerializer serializer,
            Type targetType, ref bool flag)
        {
        }

        #endregion  // Serialization

        #region Operators

        public static bool operator ==(Cell x, Cell y)
        {
            if (Object.ReferenceEquals(x, y))
            {
                return true;
            }
            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }
            return x.Equals(y);
        }

        public static bool operator !=(Cell x, Cell y)
        {
            return !(x == y);
        }

        #endregion  // Operators

        /// <summary>
        /// Supports light-weight custom type hierarchy for Cell and its subclasses.
        /// </summary>
        public class Tag
        {
            /// <summary>
            /// Gets the immediate base type tag.
            /// </summary>
            /// Returns null if this is a root tag.
            public Tag Base { get; private set; }

            /// <summary>
            /// Gets the correspondent runtime type.
            /// </summary>
            public Type RuntimeType { get; private set; }

            /// <summary>
            /// Gets the number of immediate (directly defined) properties in this type.
            /// </summary>
            public int NumProps { get; private set; }

            /// <summary>
            /// Gets the fingerprint offset for immediate properties in this type.
            /// </summary>
            public int Offset { get; private set; }

            /// <summary>
            /// Initializes a new instance of the Cell.Tag class.
            /// </summary>
            public Tag(Tag baseTag, Type runtimeType, int numProps)
            {
                Base = baseTag;
                RuntimeType = runtimeType;
                NumProps = numProps;
                if (baseTag != null)
                {
                    Offset = baseTag.Offset + baseTag.NumProps;
                }
            }
        }
    }
}
