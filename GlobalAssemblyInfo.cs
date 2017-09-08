// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System.Reflection;

[assembly: AssemblyProduct("x2net")]
[assembly: AssemblyCopyright("Copyright © 2017 Jae-jun Kang")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("0.9.12.0")]
[assembly: AssemblyFileVersion("0.9.12.0")]
