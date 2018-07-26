## TCP 소켓 클라이언트 서버

여기서는 x2net 내장 링크를 사용해 간단한 TCP 소켓 클라이언트/서버를 작성하는 방법을 살펴보겠습니다. x2net에서는 플로우/케이스 경계가 응용 프로그램 논리와 네트워크 전송 단을 효과적으로 분리하기 때문에, 응용 프로그램 논리에서 직접 Send 같은 메소드를 호출하는 여러분이 익숙한 방식과는 약간 차이가 있을 것입니다. x2net에서 모든 이벤트 처리는 바인딩 된 핸들러가 실행되는 것입니다. 그 중 우리가 링크라 부르는 어떤 케이스나 플로우들이 특정 이벤트를 처리하는 방식이 네트워크를 통해 전송하는 것일 뿐입니다.

x2net의 내장 링크 중 TCP 소켓 클라이언트/서버를 작성하기 위해 사용할 수 있는 클래스들은 다음과 같습니다.

* TCP 서버 케이스
 * `AsyncTcpServer`: SocketAsyncEventArgs 패턴을 사용하는 TCP 소켓 서버 케이스. 가능한 경우 항상 사용
 * `TcpServer`: Begin/End 패턴을 사용하는 TCP 소켓 서버 케이스. `AsyncTcpServer`를 사용할 수 없는 경우에 사용
* TCP 클라이언트 케이스
 * `AsyncTcpClient`: SocketAsyncEventArgs 패턴을 사용하는 TCP 소켓 클라이언트 케이스. 가능한 경우 항상 사용
 * `TcpClient`: Begin/End 패턴을 사용하는 TCP 소켓 클라이언트 케이스. `AsyncTcpClient`를 사용할 수 없는 경우에 사용

위의 클래스들은 모두 `Case`로부터 상속 받는 케이스 링크이므로, 여러분이 원하는 대로 임의의 플로우에 추가해 쓸 수 있습니다.

### 이벤트 준비하기

입력 문자열을 그대로 되돌려주는 에코 클라이언트/서버를 위해 다음과 같이 에코 요청/응답 두 개의 이벤트를 정의합니다.

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

그리고 에코 요청에 대한 반응으로 에코 응답을 생성하는 논리 케이스 하나를 정의합니다.

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

### TCP 소켓 서버

`AsyncTcpServer`를 상속 받아 여러분 자신의 서버 케이스를 만들고, `protected void Setup()` 메소드를 재정의해 그 안에서 클라이언트 접속 대기를 시작하도록 합니다.

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

클라이언트로부터 서버로 네트워크를 통해 `EchoReq` 이벤트가 도착하면 서버 케이스는 해당 이벤트를 허브에 포스팅합니다. 그러면 `EchoCase`의 핸들러가 실행되어 `Message`속성이 복사된 `EchoResp` 이벤트를 생성해 다시 허브에 포스팅합니다. 서버 케이스는 `EchoResp` 이벤트를 받아 클라이언트로 보냅니다.

### TCP 소켓 클라이언트

`AsyncTcpClient`를 상속 받아 여러분 자신의 클라이언트 케이스를 만들고, `protected void Setup()` 메소드를 재정의해 그 안에서 서버 연결을 시작하도록 합니다.

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

콘솔에 문자열이 입력되면 주 스레드는 해당 문자열을 `Message` 속성으로 갖는 새로운 `EchoReq` 이벤트를 생성해 허브에 포스팅 합니다.
클라이언트 케이스는 이 `EchoReq` 이벤트를 연결된 서버로 전송합니다. 서버에서 `EchoResp` 응답이 도착하면 클라이언트 케이스가 이를 허브에 포스팅 하고, `OnEchoResp` 핸들러에서 콘솔에 내용을 출력합니다.
