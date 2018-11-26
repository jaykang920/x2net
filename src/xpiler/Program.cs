﻿// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;

namespace x2net.xpiler
{
    class Program
    {
        private static Dictionary<string, Handler> handlers;
        private static Dictionary<string, Formatter> formatters;
        private static Options options;

        private bool error;
        private Formatter formatter;
        private Stack<string> subDirs;

        public static Dictionary<string, Formatter> Formatters
        {
            get { return formatters; }
        }

        public static Options Options
        {
            get { return options; }
        }

        public bool Error
        {
            get { return error; }
        }

        static Program()
        {
            options = new Options();

            handlers = new Dictionary<string, Handler>();
            handlers.Add(".xml", new CommentAwareXmlHandler());

#if YAML_HANDLER
            var yamlHandler = new YamlHandler();
            handlers.Add(".yml", yamlHandler);
            handlers.Add(".yaml", yamlHandler);
#endif

            formatters = new Dictionary<string, Formatter>();
            formatters.Add("cs", new CSharpFormatter());
        }

        public Program()
        {
            formatter = formatters[options.Spec];
            subDirs = new Stack<string>();
        }

        static int Main(string[] args)
        {
            var index = Options.Parse(args);
            if (index >= args.Length)
            {
                Console.WriteLine("{0} error: at least one input path required", System.Reflection.Assembly.GetExecutingAssembly().EntryPoint.DeclaringType.Namespace);
                return 2;
            }

            var program = new Program();
            while (index < args.Length)
            {
                program.Process(args[index++]);
            }
            return (program.Error ? 1 : 0);
        }

        public void Process(string path)
        {
            if (Directory.Exists(path))
            {
                ProcessDir(path, null);
            }
            else if (File.Exists(path))
            {
                ProcessFile(path);
            }
            else
            {
                if (path.IndexOfAny(new char[] { '*', '?' }) >= 0)
                {
                    ProcessDir(".", path);
                }
                else
                {
                    Console.Error.WriteLine("{0} doesn't exist.", path);
                    error = true;
                }
            }
        }

        private void ProcessDir(string path, string searchPattern)
        {
            Console.WriteLine("Directory {0}", Path.GetFullPath(path));
            var di = new DirectoryInfo(path);
            FileSystemInfo[] entries;
            if (ReferenceEquals(searchPattern, null))
            {
                entries = di.GetFileSystemInfos();
            }
            else
            {
                entries = di.GetFileSystemInfos(searchPattern);
            }
            foreach (var entry in entries)
            {
                var pathname = Path.Combine(path, entry.Name);
                if ((entry.Attributes & FileAttributes.Directory) != 0)
                {
                    if (options.Recursive)
                    {
                        subDirs.Push(entry.Name);
                        ProcessDir(pathname, searchPattern);
                        subDirs.Pop();
                    }
                }
                else
                {
                    ProcessFile(pathname);
                }
            }
        }

        private void ProcessFile(string path)
        {
            var filename = Path.GetFileName(path);
            var extension = Path.GetExtension(path);
            string outDir;
            if (options.OutDir == null)
            {
                outDir = Path.GetDirectoryName(path);
            }
            else
            {
                outDir = Path.Combine(options.OutDir, String.Join(
                    Path.DirectorySeparatorChar.ToString(), subDirs.ToArray()));
            }
            Handler handler;
            if (handlers.TryGetValue(extension.ToLower(), out handler) == false ||
                (!options.Forced && formatter.IsUpToDate(path, outDir)))
            {
                return;
            }

            Console.WriteLine(filename);

            Unit unit;
            if (handler.Handle(path, out unit) == false)
            {
                error = true;
            }
            if (error == true || unit == null)
            {
                return;
            }

            unit.BaseName = Path.GetFileNameWithoutExtension(path);

            if (!String.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            if (formatter.Format(unit, outDir) == false)
            {
                error = true;
            }
        }
    }
}
