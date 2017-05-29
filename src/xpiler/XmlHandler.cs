// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Xml;
using System.Xml.Linq;

namespace x2net.xpiler
{
    class XmlHandler : Handler
    {
        public bool Handle(string path, out Document doc)
        {
            doc = null;
            XDocument xml;
            try
            {
                xml = XDocument.Load(path);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }

            var rootElem = xml.Root;
            if (rootElem.Name != "x2")
            {
                // Not a valid x2 document.
                return true;
            }
            doc = new Document();
            doc.Namespace = GetAttributeValue(rootElem, "namespace");

            string comment = null;
            var node = rootElem.FirstNode;
            for ( ; node != null; node = node.NextNode)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        comment = node.ToString().Trim();
                    }
                    else
                    {
                        comment = null;
                    }
                    continue;
                }
                var elem = (XElement)node;
                switch (elem.Name.ToString())
                {
                    case "ref":
                        if (ParseReference(doc, elem) == false)
                        {
                            return false;
                        }
                        break;
                    case "consts":
                        if (ParseConsts(doc, elem, comment) == false)
                        {
                            return false;
                        }
                        break;
                    case "cell":
                    case "event":
                        if (ParseCell(doc, elem, comment) == false)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                comment = null;
            }
            return true;
        }

        private bool ParseReference(Document doc, XElement elem)
        {
            var target = GetAttributeValue(elem, "target");
            if (String.IsNullOrEmpty(target))
            {
                return false;
            }
            Reference reference = new Reference();
            reference.Target = target;
            doc.References.Add(reference);
            return true;
        }

        private bool ParseConsts(Document doc, XElement elem, string comment)
        {
            var name = GetAttributeValue(elem, "name");
            var type = GetAttributeValue(elem, "type");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            if (String.IsNullOrEmpty(type))
            {
                type = "int32";  // default type
            }
            var def = new ConstsDef();
            def.Name = name;
            def.Type = type;
            def.Comment = comment;

            string subComment = null;
            var node = elem.FirstNode;
            for ( ; node != null; node = node.NextNode)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        subComment = node.ToString().Trim();
                    }
                    else
                    {
                        subComment = null;
                    }
                    continue;
                }
                var child = (XElement)node;
                if (child.IsEmpty)
                {
                    continue;
                }
                switch (child.Name.ToString())
                {
                    case "const":
                        if (ParseConstant(def, child, subComment) == false)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                subComment = null;
            }
            doc.Definitions.Add(def);
            return true;
        }

        private bool ParseConstant(ConstsDef def, XElement elem, string comment)
        {
            var name = GetAttributeValue(elem, "name");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            var element = new ConstsDef.Constant();
            element.Name = name;
            element.Value = elem.Value.Trim();
            element.Comment = comment;
            def.Constants.Add(element);
            return true;
        }

        private bool ParseCell(Document doc, XElement elem, string comment)
        {
            var name = GetAttributeValue(elem, "name");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            var isEvent = (elem.Name == "event");
            var id = GetAttributeValue(elem, "id");
            if (isEvent && String.IsNullOrEmpty(id))
            {
                return false;
            }
            CellDef def = (isEvent ? new EventDef() : new CellDef());
            def.Name = name;
            if (isEvent)
            {
                ((EventDef)def).Id = id;
            }
            def.Base = GetAttributeValue(elem, "base");
            var local = GetAttributeValue(elem, "local");
            if (!String.IsNullOrEmpty(local) && local.EndsWith("rue"))
            {
                def.IsLocal = true;
            }
            def.Comment = comment;

            string subComment = null;
            var node = elem.FirstNode;
            for ( ; node != null; node = node.NextNode)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        subComment = node.ToString().Trim();
                    }
                    else
                    {
                        subComment = null;
                    }
                    continue;
                }
                var child = (XElement)node;
                switch (child.Name.ToString())
                {
                    case "property":
                        if (ParseCellProperty(def, child, subComment) == false)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                subComment = null;
            }
            doc.Definitions.Add(def);
            return true;
        }

        private bool ParseCellProperty(CellDef def, XElement elem, string comment)
        {
            var name = GetAttributeValue(elem, "name");
            var type = GetAttributeValue(elem, "type");
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(type))
            {
                return false;
            }
            var property = new CellDef.Property();
            property.Name = name;
            property.DefaultValue = elem.Value.Trim();
            property.Comment = comment;
            def.Properties.Add(property);

            property.TypeSpec = Types.Parse(type);
            if (property.TypeSpec == null)
            {
                return false;
            }

            return true;
        }

        private string GetAttributeValue(XElement elem, string name)
        {
            XAttribute attrib = elem.Attribute(name);
            return Object.ReferenceEquals(attrib, null) ? null : attrib.Value;
        }
    }
}
