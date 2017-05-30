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
        public bool Handle(string path, out Document doc)
        {
            doc = null;

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            using (var sr = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                var o = deserializer.Deserialize<x2>(sr);

                Console.WriteLine(o.Namespace);

                Console.WriteLine(o.Definitions.Count);

                //Console.WriteLine(((Event)o.Definitions[1]).Properties.Count);

                doc = ToDocument(o);
            }

            return true;
        }

        private Document ToDocument(x2 doc)
        {
            Document result = new Document();

            var refs = doc.References;
            if (refs != null && refs.Count != 0)
            {
                foreach (var reference in refs)
                {
                    Console.WriteLine("ref target={0}", reference.Target);
                }
            }
            var defs = doc.Definitions;
            if (refs != null && refs.Count != 0)
            {
                foreach (var def in defs)
                {
                    Console.WriteLine("def name={0}", def.Name);

                    if (def.Class == "consts")
                    {
                        Console.WriteLine(def.Elements.Count);
                    }
                    else
                    {
                        Console.WriteLine(def.Properties.Count);
                    }
                }
            }

            return result;
        }

        public class x2
        {
            public string Namespace { get; set; }

            public List<Ref> References { get; set; }

            public List<Def> Definitions { get; set; }
        }

        public class Ref
        {
            public string Target { get; set; }
        }

        public class Def
        {
            public string Class { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Id { get; set; }
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
