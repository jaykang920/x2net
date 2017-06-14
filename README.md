x2net
=====

[![NuGet](http://img.shields.io/nuget/v/x2net.svg?style=flat)](https://www.nuget.org/packages/x2net/)
[![Build status](https://img.shields.io/appveyor/ci/jaykang920/x2net.svg?style=flat)](https://ci.appveyor.com/project/jaykang920/x2net)

x2net is the reference port of [x2](https://github.com/jaykang920/x2) written in
C#, targeting universal .NET environments covering Mono, .NET framework, and .NET
Core.

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
        Bind(new EchoResp(), Send);
        Listen(6789);
    }
    
    public static void Main()
    {
        EventFactory.Resiger<EchoReq>();
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
        Bind(new EchoReq(), Send);
        Bind(new EchoResp(), resp => { Console.WriteLine(resp.Message); });
        Connect("127.0.0.1", 6789);
    }
    
    public static void Main()
    {
        EventFactory.Resiger<EchoResp>();
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

* .NET framework 3.5 or newer equivalent environment to run
* Visual Studio 2008 (9.0) or newer equivalent tool to compile C# 3.0

### .NET Core

* .NET Core 2.0 or newer
* Visual Studio 2017 (version 15.3) or newer

Installation
------------

### Binary

x2net can be installed via the [NuGet UI](https://docs.nuget.org/consume/package-manager-dialog) (as [x2net](https://www.nuget.org/packages/x2net)), or via the NuGet Package Manager console command:

    PM> Install-Package x2net

The xpiler converts x2 definition files into corresponding C# source code files.
So most probably you will want to install the [x2net.xpiler](https://www.nuget.org/packages/x2net.xpiler) too.

    PM> Install-Package x2net.xpiler

### Source

You may clone the latest source code of x2net from its [GitHub repository](https://github.com/jaykang920/x2net.git).

Zipped archives containing specific tagged versions of the source code are available in [releases](https://github.com/jaykang920/x2net/releases).

### Unity3D

If you want to use x2net in Unity3D, you should build it with the conditional
compile constant UNITY_WORKAROUND and drop the DLL into the Assets/Plugin folder.

Documentation
-------------

[x2net wiki](https://github.com/jaykang920/x2net/wiki) can be a good start point to learn how x2net applications are organized.

Contributions
-------------

We need your contributions! Please read [CONTRIBUTING.md](CONTRIBUTING.md) before you start.

License
-------

x2net is distributed under [MIT License](http://opensource.org/licenses/MIT).
See the file [LICENSE](LICENSE) for details.
