// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace x2net.xpiler
{
    public class YamlHandler : Handler
    {
        public bool Handle(string path, out Unit unit)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new HyphenatedNamingConvention())
                .Build();

            using (var sr = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                var doc = deserializer.Deserialize<Document>(sr);

                unit = Normalize(doc);
            }

            return true;
        }

        private Unit Normalize(Document doc)
        {
            Unit unit = new Unit();

            unit.Namespace = doc.Namespace;

            if (doc.References != null)
            {
                for (int i = 0; i < doc.References.Count; ++i)
                {
                    var r = doc.References[i];
                    if (!String.IsNullOrEmpty(r.Type) && r.Type.ToLower() == "namespace")
                    {
                        var reference = new Reference();
                        reference.Target = r.Target;
                        unit.References.Add(reference);
                    }
                }
            }

            if (doc.Definitions != null)
            {
                for (int i = 0; i < doc.Definitions.Count; ++i)
                {
                    var def = doc.Definitions[i];
                    if (def.Class == "cell" || def.Class == "event")
                    {
                        bool isEvent = (def.Class == "event");
                        CellDef definition = (isEvent ? new EventDef() : new CellDef());

                        definition.Name = def.Name;
                        definition.Base = def.Base;

                        if (!String.IsNullOrEmpty(def.Local) && def.Local.ToLower() == "true")
                        {
                            definition.IsLocal = true;
                        }
                        if (isEvent)
                        {
                            ((EventDef)definition).Id = def.Id;
                        }

                        if (def.Properties != null)
                        {
                            for (int j = 0; j < def.Properties.Count; ++j)
                            {
                                var p = def.Properties[j];
                                var property = new CellDef.Property();
                                property.Name = p.Name;
                                property.TypeSpec = Types.Parse(p.Type);
                                property.DefaultValue = p.Default;
                                definition.Properties.Add(property);
                            }
                        }

                        unit.Definitions.Add(definition);
                    }
                    else if (def.Class == "consts")
                    {
                        var definition = new ConstsDef();

                        definition.Name = def.Name;
						
                        var type = def.Type;
                        if (String.IsNullOrEmpty(type))
                        {
                            type = "int32";
                        }
                        definition.Type = type;

                        if (def.Elements != null)
                        {
                            for (int j = 0; j < def.Elements.Count; ++j)
                            {
                                var e = def.Elements[j];
                                var constant = new ConstsDef.Constant();
                                constant.Name = e.Name;
                                constant.Value = e.Value;
                                definition.Constants.Add(constant);
                            }
                        }

                        unit.Definitions.Add(definition);
                    }
                }
            }

            return unit;
        }

        public class Document
        {
            public string Namespace { get; set; }
            public List<Ref> References { get; set; }
            public List<Def> Definitions { get; set; }
        }

        public class Ref
        {
            public string Type { get; set; }
            public string Target { get; set; }
        }

        public class Def
        {
            public string Class { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Id { get; set; }
            public string Base { get; set; }
            public string Local { get; set; }
            public List<Element> Elements { get; set; }
            public List<Property> Properties { get; set; }
        }

        public class Element
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class Property
        {
            public string Name { get; set; }
            public string Type { get; set; }
			public string Default { get; set; }
        }
    }
}
