# Installation

## NuGet

The NuGet package [x2net](https://www.nuget.org/packages/x2net) can be installed
via the [Package Manager UI](https://docs.microsoft.com/en-us/nuget/tools/package-manager-ui),
or by the following Package Manager console command:

    PM> Install-Package x2net

The xpiler converts x2 definition files into corresponding C# source code files.
So most probably you will want to install the
[x2net.xpiler package](https://www.nuget.org/packages/x2net.xpiler) too.

    PM> Install-Package x2net.xpiler

## Source

You may `clone` or download the source code of x2net from its
[GitHub repository](https://github.com/jaykang920/x2net).

Zipped archives containing specific tagged versions of the source code are
available in [releases](https://github.com/jaykang920/x2net/releases).

## Unity3D

When you use x2net in Unity3D, you need to activate the conditional compile flag
`UNITY_WORKAROUND`. You should experience no problem using x2net with recent
versions of Unit3D running .NET Framework 4.5 or higher.

If you want to use x2net in older versions of Unity3D depending on Mono, then you
should activate the conditional compile flag `UNITY_MONO` too.
