# Hello, World!

We start with a very simple console application that prints out a greeting, accepting a name input.

```csharp
using System;

class Hello0
{
    public static void Main()
    {
        while (true)
        {
            string input= Console.ReadLine();
            if (input == "bye")
            {
                break;
            }
            Console.WriteLine("Hello, {0}!", input);
        }
    }
}
```

Yes, this is not so interesting. Though it's not very probable, let's say we are told that this application should support distribution across multiple threads, even across multiple processes in a client/server architecture. Of course, we think it's crazy, but we will see how we can meet the requirements with x2net.

## Preparing Shared Knowledge

Constants, date types and message types that are shared among participating processes of a distributable application should be defined in one or more x2 definition files, and be converted to C# source files with xpiler. Since this is a trivial example, we create a single x2 definition file, name it `HelloWorld.xml`, and define two events as follows:

```xml
<?xml version="1.0" encoding="utf-8"?>
<x2>
  <definitions>
    <event name="HelloReq" id="1">
        <property name="Name" type="string"/>
    </event>
    <event name="HelloResp" id="2">
        <property name="Message" type="string"/>
    </event>
  </definitions>
</x2>
```

In the directory where the definition file resides, we generate the 'HelloWorld.cs' source file, running xpiler.exe.

```
path_to_xpiler/x2net.xpiler.exe .
```

Bringing the generated C# source file, the two events `HelloReq` and `HelloResp` are available as C# class types.

It's usually a good idea to build the shared knowledge as a separate class library so that it can be referenced from multiple projects.

## Partitioning Application Logic

Now we define actions that works on the above messages. We pick up two key tasks in the original application, and encapsulate them into respective x2 cases.

```csharp
public class HelloCase : Case
{
    protected override void Setup()
    {
        new HelloReq().Bind(req => {
            new HelloResp {
                Message = String.Format("Hello, {0}!", req.Name)
            }.InResponseOf(req).Post();
        });
    }
}
```

```csharp
public class OutputCase : Case
{
    protected override void Setup()
    {
        new HelloResp().Bind(e => Console.WriteLine(e.Message));
    }
}
```

`HelloCase` binds a delegate handler for every `HelloReq` event. The handler creates a new `HelloResp` event in response, fills its `Result` property with the combined greeting, and posts it up to the hub.

`OutputCase` binds a delegate handler for every `CapitalizeResp` event. The handler prints out the `Result` property of the response event.

Since x2 cases might run on any flow in any process, it is also a good idea to package them into a fine-grained set of class libraries for easy re-structuring.

## Going Multithreaded

Now we can write our first x2net application, using the events and cases prepared above.

```csharp
using System;

using x2net;

class Hello1
{
    public static void Main()
    {
        Hub.Instance
            .Attach(new SingleThreadFlow()
                .Add(new HelloCase())
                .Add(new OutputCase()));

        using (new Hub.Flows().Startup())
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "bye")
                {
                    break;
                }
                new HelloReq { Name = input }.Post();
            }
        }
    }
}
```

This new application seems to work the same as the original one. But it effectively runs the greeting and output tasks in a separate thread.

Once we have built up an x2 application structure, it's quite easy to reconfigure its threading model or distribution topology, without modifying the case logic. For example, the following trivial change will introduce another thread to separate the console output task from the greeting task:

```csharp
    public static void Main()
    {
        Hub.Instance
            .Attach(new SingleThreadFlow("HelloFlow")
                .Add(new HelloCase()))
            .Attach(new SingleThreadFlow("OutputFlow")
                .Add(new OutputCase()));
        ...
```

## Going Client/Server

Now it is not so difficult to distribute the application execution through a client/server topology. Please note that the logic cases remain unchanged, while we re-write the other part of the code supporting communication.

More detailed discussion can be found in [[TCP Socket Client Server]].

### Hello Server

```csharp
using System;

using x2net;

class Hello2Server
{
    class HelloServer : AsyncTcpServer
    {
        public HelloServer() : base("HelloServer") { }

        protected override void Setup()
        {
            EventFactory.Register<HelloReq>();
            new HelloResp().Bind(Send);
            Listen(6789);
        }
    }

    public static void Main()
    {
        Hub.Instance
            .Attach(new SingleThreadFlow()
                .Add(new HelloCase())
                .Add(new HelloServer()));

        using (new Hub.Flows().Startup())
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "bye")
                {
                    break;
                }
            }
        }
    }
}
```

### Hello Client

```csharp
using System;

using x2net;

class Hello2Client
{
    class HelloClient : TcpClient
    {
        public HelloClient() : base("HelloClient") { }

        protected override void Setup()
        {
            EventFactory.Register<HelloResp>();
            new HelloReq().Bind(Send);
            Connect("127.0.0.1", 6789);
        }
    }

    public static void Main()
    {
        Hub.Instance
            .Attach(new SingleThreadFlow()
                .Add(new OutputCase())
                .Add(new HelloClient()));

        using (new Hub.Flows().Startup())
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "bye")
                {
                    break;
                }
                new HelloReq { Name = input }.Post();
            }
        }
    }
}
```
