// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace x2net.xpiler
{
    [XmlRoot("x2")]
    public class x2
    {
        [XmlAttribute("namespace")]
        public string Namespace { get; set; }

        [XmlArray("references")]
        [XmlArrayItem("reference", typeof(Ref))]
        public List<Ref> References { get; set; }

        [XmlArray("definitions")]
        [XmlArrayItem("cell", Type=typeof(Cell))]
        [XmlArrayItem("event", Type=typeof(Event))]
        public List<Def> Definitions { get; set; }
    }

    public class Ref
    {
        [XmlAttribute("target")]
        public string Target { get; set; }
    }

    public class Def
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }

    public class Cell : Def
    {
        [XmlArray("properties")]
        [XmlArrayItem("property", Type=typeof(Property))]
        public List<Property> Properties { get; set; }
    }

    public class Event : Cell
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
    }

    public class Property
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }
    }

    class XmlSerializerHandler : Handler
    {
        public bool Handle(string path, out Document doc)
        {
            doc = null;

            XmlSerializer serializer = new XmlSerializer(typeof(x2));
            using (var fs = new FileStream(path, FileMode.Open))
            {
                x2 o = (x2)serializer.Deserialize(fs);

                Console.WriteLine(o.Namespace);

                Console.WriteLine(o.Definitions.Count);

                Console.WriteLine(((Event)o.Definitions[1]).Properties.Count);
            }

            return true;
        }
    }
}
