**x2net**
=========

[![NuGet](http://img.shields.io/nuget/v/x2net.svg?style=flat)](https://www.nuget.org/packages/x2net/)
[![Build status](https://img.shields.io/appveyor/ci/jaykang920/x2net.svg?style=flat)](https://ci.appveyor.com/project/jaykang920/x2net)

*x2net* is the reference port of [x2](https://github.com/jaykang920/x2) written
in C#, targeting universal [.NET](https://www.microsoft.com/net) environments
covering Mono, .NET Framework, and .NET Core.

In order to make full use of x2net, you need to understand the fundamental
concepts and ideas of x2. At least, you should be familiar with basic
[x2 concepts](https://github.com/jaykang920/x2/blob/master/concepts.md)
such as cell, event, hub, flow, case and link.

Features
--------

### Distributable Application Architecture

Writing distributed (including client/server) applications has never been this
easy. You can flexibly make changes to the deployment topology of your
application, while your business logic remains unchanged.

### Communication Protocol Code Generation

xpiler converts your shared knowledge definitions to corresponding C# source
code files. Relying on the knowledge shared among application participants, x2
wire format comes extremely efficient.

### Advanced Event-Driven Programming Support

* Hierarchical, self-descriptive events
* Precise handler binding with multi-property pattern matching
* Time-deferred or periodic event supply
* Coroutines to join multiple sequential event handlers

Example
-------

Here we develop a simple TCP echo client/server, starting with defining two events.

```xml
<x2>
  <definitions>
    <event name="EchoReq" type="1">
        <property name="Message" type="string"/>
    </event>
    <event name="EchoResp" type="2">
        <property name="Message" type="string"/>
    </event>
  </definitions>
</x2>
```

And we encapsulate our core function (generating echo response) into a case.
Please note that this logic case knows nothing about the communication detail.

```csharp
public EchoCase : Case
{
    protected override void Setup()
    {
        Bind(new EchoReq(),
            req => { new EchoResp { Message = req.Message }.InResponseOf(req).Post(); });
    }
}
```

Now we extend builtin TCP link cases to handle the network communication.

```csharp
public EchoServer : AsyncTcpServer
{
    public EchoServer() : base("EchoServer") { }

    protected override void Setup()
    {
        EventFactory.Resiger<EchoReq>();
        Bind(new EchoResp(), Send);
        Listen(6789);
    }

    public static void Main()
    {
        Hub.Instance
            .Attach(new SingleThreadFlow()
                .Add(new EchoCase())
                .Add(new EchoServer());
        using (new Hub.Flows().Startup())
        {
            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }
            }
        }
    }
}

public EchoClient : AsyncTcpClient
{
    public EchoClient() : base("EchoClient") { }

    protected override void Setup()
    {
        EventFactory.Resiger<EchoResp>();
        Bind(new EchoReq(), Send);
        Bind(new EchoResp(), resp => { Console.WriteLine(resp.Message); });
        Connect("127.0.0.1", 6789);
    }

    public static void Main()
    {
        Hub.Instance
            .Attach(new SingleThreadFlow()
                .Add(new EchoClient()));
        using (new Hub.Flows().Startup())
        {
            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }
                else
                {
                    new EchoReq { Message = message }.Post();
                }
            }
        }
    }
}
```

Requirements
------------

### .NET Framework and Mono

* .NET framework 3.5 or newer equivalent environment to run
* Visual Studio 2008 (9.0) or newer equivalent tool to compile C# 3.0

### .NET Core

* .NET Core 2.0 or newer
* Visual Studio 2017 (version 15.3) or newer

Installation
------------

The following two NuGet packages can be installed via the
[Package Manager UI](https://docs.microsoft.com/en-us/nuget/tools/package-manager-ui),
or by the Package Manager console command `Install-Package`.

* [x2net](https://www.nuget.org/packages/x2net)
* [x2net.xpiler](https://www.nuget.org/packages/x2net.xpiler)

See [Installation](https://jaykang920.github.io/x2net/articles/en/install.html)
for more about installation.

Documentation
-------------

[Tutorials & Guides](https://jaykang920.github.io/x2net/articles/index.html) is
a good start point to learn how x2net applications are organized.

Contributions
-------------

We need your contributions! Please read
[CONTRIBUTING.md](https://github.com/jaykang920/x2net/blob/master/CONTRIBUTING.md)
before you start.

License
-------

x2net is distributed under [MIT License](http://opensource.org/licenses/MIT).
See the file [LICENSE](https://github.com/jaykang920/x2net/blob/master/LICENSE)
for details.
