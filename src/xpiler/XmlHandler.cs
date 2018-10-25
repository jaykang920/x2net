// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace x2net.xpiler
{
    public class XmlHandler : Handler
    {
        public bool Handle(string path, out Unit unit)
        {
            unit = null;
            Root root;

            var serializer = new XmlSerializer(typeof(Root));

            using (var fs = new FileStream(path,
                FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    root = (Root)serializer.Deserialize(fs);
                }
                catch (InvalidOperationException)
                {
                    // not a valid x2 document
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            unit = Normalize(root);

            return true;
        }

        private Unit Normalize(Root root)
        {
            Unit unit = new Unit {
                Namespace = root.Namespace
            };

            if (root.References != null)
            {
                for (int i = 0; i < root.References.Count; ++i)
                {
                    var r = root.References[i];
                    if (r.GetType() == typeof(NamespaceRef))
                    {
                        var reference = new Reference {
                            Target = r.Target
                        };
                        unit.References.Add(reference);
                    }
                }
            }

            if (root.Definitions != null)
            {
                for (int i = 0; i < root.Definitions.Count; ++i)
                {
                    var def = root.Definitions[i];
                    if (def is Cell)
                    {
                        bool isEvent = (def.GetType() == typeof(Event));
                        Cell c = (Cell)def;
                        CellDef definition = (isEvent ? new EventDef() : new CellDef());

                        definition.Name = c.Name;
                        definition.Base = c.Base;

                        if (!String.IsNullOrEmpty(c.Local) && c.Local.ToLower() == "true")
                        {
                            definition.IsLocal = true;
                        }
                        if (!String.IsNullOrEmpty(c.AsIs) && c.AsIs.ToLower() == "true")
                        {
                            definition.AsIs = true;
                        }
                        if (isEvent)
                        {
                            ((EventDef)definition).Id = ((Event)c).Id;
                        }

                        if (c.Properties != null)
                        {
                            for (int j = 0; j < c.Properties.Count; ++j)
                            {
                                var p = c.Properties[j];
                                var property = new CellDef.Property {
                                    Name = p.Name,
                                    TypeSpec = Types.Parse(p.Type),
                                    DefaultValue = p.Default
                                };
                                definition.Properties.Add(property);
                            }
                        }

                        unit.Definitions.Add(definition);
                    }
                    else if (def.GetType() == typeof(Consts))
                    {
                        var c = (Consts)def;
                        var definition = new ConstsDef {
                            Name = c.Name
                        };

                        var type = c.Type;
                        if (String.IsNullOrEmpty(type))
                        {
                            type = "int32";
                        }
                        definition.Type = type;

                        if (c.Elements != null)
                        {
                            for (int j = 0; j < c.Elements.Count; ++j)
                            {
                                var e = c.Elements[j];
                                var constant = new ConstsDef.Constant {
                                    Name = e.Name,
                                    Value = e.Value
                                };
                                definition.Constants.Add(constant);
                            }
                        }

                        unit.Definitions.Add(definition);
                    }
                }
            }

            return unit;
        }

        [XmlRoot("x2")]
        public class Root
        {
            [XmlAttribute("namespace")]
            public string Namespace { get; set; }

            [XmlArray("references")]
            [XmlArrayItem("namespace", Type = typeof(NamespaceRef))]
            [XmlArrayItem("file", Type = typeof(FileRef))]
            public List<Ref> References { get; set; }

            [XmlArray("definitions")]
            [XmlArrayItem("consts", Type = typeof(Consts))]
            [XmlArrayItem("cell", Type = typeof(Cell))]
            [XmlArrayItem("event", Type = typeof(Event))]
            public List<Def> Definitions { get; set; }
        }

        public class Ref
        {
            [XmlAttribute("target")]
            public string Target { get; set; }
        }

        public class NamespaceRef : Ref { }

        public class FileRef : Ref { }

        public class Def
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        public class Consts : Def
        {
            [XmlAttribute("type")]
            public string Type { get; set; }
            [XmlElement("const", Type = typeof(Element))]
            public List<Element> Elements { get; set; }
        }

        public class Cell : Def
        {
            [XmlAttribute("base")]
            public string Base { get; set; }
            [XmlAttribute("local")]
            public string Local { get; set; }
            [XmlAttribute("asIs")]
            public string AsIs { get; set; }
            [XmlElement("property", Type = typeof(Property))]
            public List<Property> Properties { get; set; }
        }

        public class Event : Cell
        {
            [XmlAttribute("id")]
            public string Id { get; set; }
        }

        public class Element
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlText]
            public string Value { get; set; }
        }

        public class Property
        {
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlAttribute("type")]
            public string Type { get; set; }
            [XmlText]
            public string Default { get; set; }
        }
    }
}
