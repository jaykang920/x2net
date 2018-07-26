## TCP Socket Client Server

You can build up a simple TCP socket client/server application with x2net builtin links. Since x2net effectively separates the network transmission details from the application logic, you might see that it's somewhat different from the way you are accustomed to: calling a Send-like method within the application logic. In x2net, there are some cases or flows, so-called links, that send/receive events to/from a network. The key point is that your application logic remains unchanged while your networking details change.

Follows the x2net builtin links that you can use to write a TCP socket client/server:

* TCP server cases
 * `AsyncTcpServer`: TCP socket server case based on SocketAsyncEventArgs pattern. Use this whenever possible.
 * `TcpServer`: TCP socket server case based on Begin/End pattern. Use this when `AsyncTcpServer` is not feasible.
* TCP client cases
 * `AsyncTcpClient`: TCP socket client case based on SocketAsyncEventArgs pattern. Use this whenever possible.
 * `TcpClient`: TCP socket client case based on Begin/End pattern. Use this when `AsyncTcpClient` is not feasible.

All the above classes inherit from `Case`, and you may add them to any flow you want.

### Preparing Events

Let's start with defining the two events, echo request and response, and deriving an application logic case from `Case`.

```xml
<x2>
    <event name="EchoReq" type="1">
        <property name="Message" type="string"/>
    </event>
    <event name="EchoResp" type="2">
        <property name="Message" type="string"/>
    </event>
</x2>
```

```csharp
public class EchoCase : Case
{
    protected void Setup()
    {
        Bind(new EchoReq(), OnEchoReq);
    }

    void OnEchoReq(EchoReq req)
    {
        new EchoResp {
            Message = req.Message
        }.InResponseOf(req).Post();
    }
}
```

### TCP Socket Server

Write your own server case derived from `AsyncTcpServer`, and override `protected void Setup()` method to start listening.

```csharp
public class ServerCase : AsyncTcpServer
{
    public ServerCase() : base("EchoServer") { }

    protected override void Setup()
    {
        EventFactory.Register<EchoReq>();
        Bind(new EchoResp(), Send);
        Listen(6789);
    }
}

public class EchoServer
{
    public static void Main()
    {
        Hub.Instance
            .Attach(new SingleThreadFlow()
                .Add(new EchoCase())
                .Add(new ServerCase()));

        using (new Hub.Flows().Startup())
        {
            while (true)
            {
                string message = Console.ReadLine();
                if (message = "quit")
                {
                    break;
                }
            }
        }
    }
}
```

When the server case receives an `EchoReq` event from the client socket, it posts up the event to the hub. Then the handler in `EchoCase` runs to create a new `EchoResp` event holding the same `Message` property as the request, and posts the response event to the hub. The server case receives the `EchoResp` event and sends it to the client.

### TCP Socket Client

Write your own client case derived from `AsyncTcpClient`, and override `protected void Setup()` method to start connecting.

```csharp
public class ClientCase : AsyncTcpClient
{
    public ClientCase() : base("ClientCase") { }

    protected override void Setup()
    {
        EventFactory.Register<EchoResp>();
        Bind(new EchoReq(), Send);
        Bind(new EchoResp(), OnEchoResp);
        Connect("127.0.0.1", 6789);
    }

    void OnEchoResp(EchoResp e)
    {
        Console.WriteLine(e.Message);
    }
}

public class EchoClient
{
    public static void Main()
    {
        Hub.Instance
            .Attach(new SingleThreadFlow()
                .Add(new ClientCase()));

        using (new Hub.Flows().Startup())
        {
            while (true)
            {
                string message = Console.ReadLine();
                if (message = "quit")
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

When a text string input is ready at the console, the main thread creates a new `EchoReq` event and posts it up to the hub. The client case receives the `EchoReq` event and sends it to the server. When a `EchoResp` event arrives from the server, the client case posts the response event to the hub. Now the `EchoResp` event is available for local handlers.


