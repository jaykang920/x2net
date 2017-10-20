// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net.xpiler
{
    class Options
    {
        private const string DefaultSpec = "cs";

        public bool Forced { get; private set; }
        public string OutDir { get; private set; }
        public bool Recursive { get; private set; }
        public string Spec { get; private set; }

        public Options()
        {
            Spec = DefaultSpec;
        }

        public int Parse(string[] args)
        {
            var longopts = new Getopt.Option[]
            {
                new Getopt.Option("spec", Getopt.RequiredArgument, 's'),
                new Getopt.Option("out-dir", Getopt.RequiredArgument, 'o'),
                new Getopt.Option("recursive", Getopt.NoArgument, 'r'),
                new Getopt.Option("force", Getopt.NoArgument, 'f'),
                new Getopt.Option("help", Getopt.NoArgument, 'h')
            };

            var getopt = new Getopt(args, "s:o:rfh", longopts);
            while (getopt.Next() != -1)
            {
                switch (getopt.Opt)
                {
                    case 's':
                        Spec = getopt.OptArg.ToLower();
                        if (!Program.Formatters.ContainsKey(Spec))
                        {
                            Console.Error.WriteLine(
                                "error: unknown target formatter specified - {0}",
                                Spec);
                            System.Environment.Exit(2);
                        }
                        break;
                    case 'o':
                        OutDir = getopt.OptArg;
                        break;
                    case 'r':
                        Recursive = true;
                        break;
                    case 'f':
                        Forced = true;
                        break;
                    case 'h':
                        PrintUsage();
                        System.Environment.Exit(2);
                        break;
                    default:
                        break;
                }
            }
            return getopt.OptInd;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("usage: x2net.xpiler (options) [path...]");
            Console.WriteLine(" options:");
            Console.WriteLine("  -f (--force)       : force all to be re-xpiled");
            Console.WriteLine("  -h (--help)        : print this message and quit");
            Console.WriteLine("  -o (--out-dir) dir : specifies the output root directory");
            Console.WriteLine("  -r (--recursive)   : process subdirectories recursively");
            Console.WriteLine("  -s (--spec) spec   : specifies the target formatter");

            foreach (var pair in Program.Formatters)
            {
                Console.Write("{0,20} : {1}", pair.Key, pair.Value.Description);
                if (pair.Key == DefaultSpec)
                {
                    Console.Write(" (default)");
                }
                Console.WriteLine();
            }
        }
    }
}
