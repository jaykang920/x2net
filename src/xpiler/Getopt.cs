// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.IO;
using System.Reflection;

namespace x2net.xpiler
{
    /// <summary>Getopt is a clone of the GNU C getopt.</summary>
    public class Getopt
    {
        /// <summary>Indicates that the option does not take an argument.</summary>
        public const int NoArgument = 0;
        /// <summary>Indicates that the option requires an argument.</summary>
        public const int RequiredArgument = 1;
        /// <summary>Indicates that the option takes an optional argument.</summary>
        public const int OptionalArgument = 2;

        public class Option
        {
            private readonly string name;
            private readonly int hasArg;
            private readonly int value;

            public string Name { get { return name; } }
            public int HasArg { get { return hasArg; } }
            public int Value { get { return value; } }

            public Option(string name, int hasArg, int value)
            {
                this.name = name;
                this.hasArg = hasArg;
                this.value = value;
            }
        }

        private enum Ordering
        {
            Permute = 0,
            ReturnInOrder,
            RequireOrder
        }

        private string[] args;
        private string optstring;
        private Option[] longopts;

        private bool done;
        private int opt;

        private string optarg;
        private int optind;
        private int optopt;
        private bool opterr;

        private int longIndex;
        private bool longOnly;

        private string nextchar;
        private int firstNonopt;
        private int lastNonopt;

        private Ordering ordering;

        private bool posixlyCorrect;

        /// <summary>
        /// Returns the last option character.
        /// </summary>
        /// <remarks>
        /// This enables the following usage:
        /// <code>
        ///    while (getopt.Next() != -1) {
        ///      int c = getopt.Opt;
        ///      ...
        ///    }
        /// </code>
        /// </remarks>
        public int Opt { get { return opt; } }

        /// <summary>
        /// Gets the value of the option argument, for those options that accept an
        /// argument.
        /// </summary>
        public string OptArg { get { return optarg; } }

        /// <summary>
        /// Gets the index of the next element to be processed in args.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The initial value is 0. Unlike C getopt, optind is a read-only property
        /// of Getopt, and you should use Reset() method in order to prepare Getopt
        /// for a new scanning, instead of resetting optind directly.
        /// </para>
        /// <para>
        /// Once Getopt has processed all the options, you can use optind to determine
        /// where the remaining non-options begin in args.
        /// </para>
        /// </remarks>
        public int OptInd { get { return optind; } }

        /// <summary>
        /// Gets the option character, when Getopt encounters an unknown option
        /// character or an option without a required argument.
        /// </summary>
        public int OptOpt { get { return optopt; } }

        /// <summary>
        /// Gets or sets whether Getopt would print out error messages or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If opterr is true (default), then Getopt prints an error message to the
        /// standard error stream if it encounters an unknown option or an option
        /// without a required argument.
        /// </para>
        /// <para>
        /// If you set opterr to false, Getopt does not print any messages, but it
        /// still returns the character '?' to indicate an error.
        /// </para>
        /// </remarks>
        public bool OptErr
        {
            get { return opterr; }
            set { opterr = value; }
        }

        /// <summary>
        /// Gets the index of the log option relative to longopts.
        /// </summary>
        /// <remarks>
        /// A negative integer means that there is no long option matched.
        /// </remarks>
        public int LongIndex { get { return longIndex; } }

        /// <summary>
        /// Gets or sets whether this Getopt would run in long_only mode.
        /// </summary>
        /// <remarks>
        /// <para>It's false by default.</para>
        /// <para>
        /// If long_only is true, '-' as well as '--' can indicate a long option.
        /// If an option that starts with '-' (not '--') matches a short option, not
        /// a long option, then it is parsed as a short option.
        /// </para>
        /// </remarks>
        public bool LongOnly
        {
            get { return longOnly; }
            set { longOnly = value; }
        }

        public Getopt(string[] args, string optstring)
            : this(args, optstring, null)
        {
        }

        public Getopt(string[] args, string optstring, Option[] longopts)
        {
            opterr = true;
            longOnly = false;

            posixlyCorrect =
                (Environment.GetEnvironmentVariable("POSIXLY_CORRECT") != null);

            Reset(args, optstring, longopts);
        }

        /// <summary>
        /// Returns the next option character.
        /// </summary>
        /// <remarks>
        /// When no more option is available, it returns -1. There may still be more
        /// non-options remaining.
        /// </remarks>
        public int Next()
        {
            optarg = null;
            optopt = '?';
            longIndex = -1;

            if (done)
            {
                return (opt = -1);
            }

            if (string.IsNullOrEmpty(nextchar))
            {
                if (LocateNext())
                {
                    return opt;
                }
            }

            if (longopts != null &&
                (args[optind].StartsWith("--") || (longOnly &&
                (args[optind].Length > 2 || (optstring.IndexOf(args[optind][1]) < 0)))))
            {
                if (CheckLong())
                {
                    return opt;
                }
            }
            return (opt = CheckShort());
        }

        public void Reset(string[] args, string optstring)
        {
            Reset(args, optstring, null);
        }

        public void Reset(string[] args, string optstring, Option[] longopts)
        {
            if (args == null || optstring == null)
            {
                throw new ArgumentNullException();
            }
            this.args = args;
            this.optstring = optstring;
            this.longopts = longopts;

            done = false;
            opt = -1;
            optarg = null;
            optind = 0;
            optopt = '?';
            longIndex = -1;

            nextchar = null;
            firstNonopt = lastNonopt = 0;

            if (optstring.StartsWith("-"))
            {
                ordering = Ordering.ReturnInOrder;
                optstring = optstring.Substring(1);
            }
            else if (optstring.StartsWith("+"))
            {
                ordering = Ordering.RequireOrder;
                optstring = optstring.Substring(1);
            }
            else if (posixlyCorrect)
            {
                ordering = Ordering.RequireOrder;
            }
            else
            {
                ordering = Ordering.Permute;
            }
        }

        private bool LocateNext()
        {
            if (ordering == Ordering.Permute)
            {
                if (lastNonopt != optind)
                {
                    if (firstNonopt != lastNonopt)
                    {
                        Permute();
                    }
                    else
                    {
                        firstNonopt = optind;
                    }
                }
                // Skip any additional non-options.
                while (optind < args.Length &&
                    (!args[optind].StartsWith("-") || args[optind].Length == 1))
                {
                    ++optind;
                }
                lastNonopt = optind;
            }

            // The special option '--' immediately stops the option scanning.
            if (optind < args.Length && args[optind] == "--")
            {
                ++optind;
                if (firstNonopt == lastNonopt)
                {
                    firstNonopt = optind;
                }
                else if (lastNonopt != optind)
                {
                    Permute();
                }
                optind = lastNonopt = args.Length;
            }

            if (optind >= args.Length)
            {
                if (firstNonopt != lastNonopt)
                {
                    optind = firstNonopt;  // Let optind point at the first non-option.
                }
                opt = -1;
                return (done = true);
            }

            // Handle a non-option for non-permute ordering.
            if (!args[optind].StartsWith("-") || args[optind].Length == 1)
            {
                if (ordering == Ordering.RequireOrder)
                {
                    opt = -1;
                    return true;
                }
                opt = 1;
                optarg = args[optind++];
                return true;
            }

            // Pick out the next option.
            nextchar = args[optind].Substring(1);  // '-'
            if (longopts != null && nextchar.StartsWith("-"))
            {
                nextchar = nextchar.Substring(1);  // '--'
            }
            return false;
        }

        private bool CheckLong()
        {
            // Search for the end of option name.
            int index = nextchar.IndexOf('=');
            string name = (index < 0 ? nextchar : nextchar.Substring(0, index));

            // Scan the long option table.
            for (int i = 0; i < longopts.Length; ++i)
            {
                Option option = longopts[i];
                if (option.Name.StartsWith(name))
                {
                    if (option.Name == name)
                    {  // exact match
                        longIndex = i;
                        break;
                    }
                    else
                    {  // non-exact match
                        if (longIndex < 0)
                        {
                            longIndex = i;
                        }
                        else
                        {
                            longIndex = -2;
                            break;
                        }
                    }
                }
            }

            if (longIndex > -1)
            {  // found
                Option option = longopts[longIndex];
                ++optind;
                if (index > 0)
                {
                    if (option.HasArg != 0)
                    {
                        optarg = nextchar.Substring(index + 1);
                    }
                    else
                    {
                        if (opterr)
                        {
                            Console.Error.WriteLine(
                                "{0}: option '{1}' doesn't allow an argument",
                                Path.GetFileName(Assembly.GetEntryAssembly().Location),
                                args[optind - 1]
                                );
                        }
                        nextchar = null;
                        opt = '?';
                        return true;
                    }
                }
                else if (option.HasArg == RequiredArgument)
                {
                    if (optind < args.Length)
                    {
                        optarg = args[optind++];
                    }
                    else
                    {
                        if (opterr)
                        {
                            Console.Error.WriteLine(
                                "{0}: option '{1}' requires an argument",
                                Path.GetFileName(Assembly.GetEntryAssembly().Location),
                                args[optind - 1]
                                );
                        }
                        nextchar = null;
                        opt = (optstring.StartsWith(":") ? ':' : '?');
                        return true;
                    }
                }
                nextchar = null;
                opt = option.Value;
                return true;
            }
            else if (longIndex < -1)
            {  // ambiguous
                if (opterr)
                {
                    Console.Error.WriteLine(
                        "{0}: option '{1}' is ambiguous",
                        Path.GetFileName(Assembly.GetEntryAssembly().Location),
                        args[optind]
                        );
                }
                nextchar = null;
                opt = '?';
                optopt = 0;
                ++optind;
                return true;
            }
            // No match found.
            if (longOnly == false || args[optind].StartsWith("--") ||
                optstring.IndexOf(nextchar[0]) < 0)
            {
                if (opterr)
                {
                    Console.Error.WriteLine(
                        "{0}: unrecognized option '{1}'",
                        Path.GetFileName(Assembly.GetEntryAssembly().Location),
                        args[optind]
                        );
                }
                nextchar = null;
                opt = '?';
                optopt = 0;
                ++optind;
                return true;
            }
            return false;
        }

        private int CheckShort()
        {
            char c = nextchar[0];
            nextchar = nextchar.Substring(1);
            string optstr;
            int index = optstring.IndexOf(c);
            if (index >= 0)
            {
                optstr = optstring.Substring(index);
            }
            else
            {
                optstr = null;
            }

            // Increment optind, in advance, on the last character.
            if (nextchar.Length == 0)
            {
                ++optind;
            }

            // Sift out invalid options.
            if (optstr == null || c == ':')
            {
                if (opterr)
                {
                    Console.Error.WriteLine(
                        "{0}: {1} option -- {2}",
                        Path.GetFileName(Assembly.GetEntryAssembly().Location),
                        (posixlyCorrect ? "illegal" : "invalid"),
                        c
                        );
                }
                optopt = c;
                return '?';
            }

            // Check for an additional argument.
            if (optstr.Length > 1 && optstr[1] == ':')
            {
                if (optstr.Length > 2 && optstr[2] == ':')
                {  // optional
                    if (nextchar.Length > 0)
                    {
                        optarg = nextchar;  // take the rest
                        ++optind;
                    }
                }
                else
                {  // required
                    if (nextchar.Length > 0)
                    {
                        optarg = nextchar;  // take the rest
                        ++optind;
                    }
                    else if (optind >= args.Length)
                    {
                        if (opterr)
                        {
                            Console.Error.WriteLine(
                                "{0}: option requires an argument -- {1}",
                                Path.GetFileName(Assembly.GetEntryAssembly().Location),
                                c);
                        }
                        optopt = c;
                        return (optstring.StartsWith(":") ? ':' : '?');
                    }
                    else
                    {
                        optarg = args[optind++];  // take the next
                    }
                }
                nextchar = null;
            }
            return c;
        }

        private void Permute()
        {
            int first = firstNonopt, middle = lastNonopt, last = optind;
            int next = middle;

            while (first != next)
            {
                string temp = args[first];
                args[first++] = args[next];
                args[next++] = temp;

                if (next == last)
                {
                    next = middle;
                }
                else if (first == middle)
                {
                    middle = next;
                }
            }

            firstNonopt += (optind - lastNonopt);
            lastNonopt = optind;
        }
    }
}
