// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System.Collections.Generic;

namespace x2net.xpiler
{
    /// <summary>
    /// Represents a single definition document.
    /// </summary>
    public class Document
    {
        public string BaseName { get; set; }
        public string Namespace { get; set; }

        public IList<Reference> References { get { return references; } }
        public IList<Definition> Definitions { get { return definitions; } }

        private IList<Reference> references = new List<Reference>();
        private IList<Definition> definitions = new List<Definition>();
    }
}
