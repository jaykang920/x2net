// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System.Collections.Generic;

namespace x2net.xpiler
{
    /// <summary>
    /// Represents a reference specification.
    /// </summary>
    class Reference
    {
        public string Target { get; set; }

        public void Format(FormatterContext context)
        {
            context.FormatReference(this);
        }
    }

    /// <summary>
    /// Represents an abstract definition.
    /// </summary>
    abstract class Definition
    {
        public string Name { get; set; }

        public abstract void Format(FormatterContext context);
    }

    /// <summary>
    /// Represents a set of constants.
    /// </summary>
    class ConstsDef : Definition
    {
        /// <summary>
        /// Represents a constant definition.
        /// </summary>
        public class Constant
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public string Comment { get; set; }
        }

        public string Type { get; set; }
        public string NativeType { get; set; }
        public List<Constant> Constants { get { return constants; } }

        public string Comment { get; set; }

        private List<Constant> constants = new List<Constant>();

        public override void Format(FormatterContext context)
        {
            context.FormatConsts(this);
        }
    }

    /// <summary>
    /// Represents a cell definition.
    /// </summary>
    class CellDef : Definition
    {
        /// <summary>
        /// Represents a cell property.
        /// </summary>
        public class Property
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public TypeSpec TypeSpec { get; set; }
            public string DefaultValue { get; set; }
            public string NativeName { get; set; }
            public string NativeType { get; set; }

            public string Comment { get; set; }
        }

        public string Base { get; set; }
        public string BaseClass { get; set; }
        public virtual bool IsEvent { get { return false; } }
        public bool IsLocal { get; set; }
        public List<Property> Properties { get { return properties; } }
        public bool HasProperties { get { return (properties.Count != 0); } }

        public string Comment { get; set; }

        private List<Property> properties = new List<Property>();

        public override void Format(FormatterContext context)
        {
            context.FormatCell(this);
        }
    }

    /// <summary>
    /// Represents an event definition.
    /// </summary>
    class EventDef : CellDef
    {
        public string Id { get; set; }

        public override bool IsEvent { get { return true; } }
    }
}
