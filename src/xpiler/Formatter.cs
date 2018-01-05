// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.IO;

namespace x2net.xpiler
{
    /// <summary>
    /// Abstract base class for output file formatters.
    /// </summary>
    abstract class Formatter
    {
        public abstract string Description { get; }

        public abstract bool Format(Unit unit, String outDir);

        public abstract bool IsUpToDate(string path, string outDir);
    }

    /// <summary>
    /// Abstract base class for concrete formatter contexts.
    /// </summary>
    public abstract class FormatterContext
    {
        public Unit Unit { get; set; }
        public TextWriter Writer { get; set; }

        public abstract void FormatCell(CellDef def);
        public abstract void FormatConsts(ConstsDef def);
        public abstract void FormatReference(Reference def);
    }
}
                                                                                                                                                                                                 