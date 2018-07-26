# Hello, World!

이름을 입력 받아 인사말을 출력하는 아주 간단한 C# 콘솔 응용 프로그램으로 시작해 봅니다.

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

보시다시피 이것 자체는 아주 재미있는 코드는 아닙니다. 그럴 리는 없겠지만, 만약 누군가 이 프로그램이 다중 스레드로, 심지어 클라이언트/서버 구조로 분산되어야 한다고 요구한다면, x2net을 이용해 이 요구를 어떻게 충족시킬 수 있는지 보겠습니다.

## 공통의 지식 준비하기

분산될 수 있는 응용 프로그램 참가 프로세스들 사이에 공유되는 상수, 데이터 형식과 메시지 형식들은 하나 이상의 정의 파일에 선언되고, xpiler에 의해 C# 소스 코드로 변환되어야 합니다. 이 예제는 아주 작은 프로그램이므로, 우리는 `HelloWorld.xml`이라는 정의 파일 하나를 만들어 다음과 같이 두 개의 이벤트들을 정의합니다:

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

정의 파일이 위치한 디렉토리에서 다음과 같이 xpiler.exe를 실행해 같은 위치에 `HelloWorld.cs` 소스 파일을 생성합니다.

```
path_to_xpiler/x2net.xpiler.exe .
```

생성된 소스 파일을 프로젝트로 가져오면, `HelloReq`와 `HelloResp` 두 개의 이벤트들을 C# 클래스 형식으로 사용할 수 있습니다.

일반적으로, 공유되는 지식은 여러 프로젝트에서 참조될 수 있도록 별도의 클래스 라이브러리로 만드는 것이 좋습니다.

### 응용 프로그램 논리 분할하기

이제 위에서 정의한 이벤트들에 대한 액션들을 정의합니다. 원래의 응용 프로그램서 두 개의 핵심 태스크를 골라 각각 다음 두 개의 x2 케이스들로 캡슐화합니다.

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

`HelloCase`는 모든 `HelloReq` 이벤트에 대해 대리자 핸들러를 바인딩합니다. 그 핸들러는 요청에 대한 응답으로 새로운 `HelloResp` 이벤트를 생성하고, `Result` 속성을 조합된 인사말로 채워 허브에 포스팅 합니다.

`OutputCase`는 모든 `HelloResp` 이벤트에 대해 대리자 핸들러를 바인딩합니다. 그 핸들러는 응답의 `Result` 속성의 내용을 콘솔레 출력합니다.

x2 케이스들은 응용 프로그램 내의 어느 프로세스, 어느 플로우에서든 실행될 수 있기 때문에, 쉬운 재구성을 위해서 그것들을 잘 분할된 클래스 라이브러리 집합으로 묶어두는 것이 좋습니다.

## 다중 스레딩

위에서 준비한 이벤트와 케이스 들을 사용해 첫 번째 x2net 응용 프로그램을 작성할 수 있습니다.

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

이 새로운 응용 프로그램은 원래의 것과 동일하게 동작하는 것처럼 보이지만, Hello 인사말을 조합하고 콘솔에 출력하는 태스크들은 사실 메인 스레드와는 다른 별도의 스레드에서 실행됩니다.

일단 x2 응용 프로그램 구조를 구축하고 나면, 케이스 내부 논리를 수정하지 않고도 스레딩 모델이나 분산 토폴로지를 쉽게 재구성할 수 있습니다. 예를 들어, 다음과 같은 사소한 변경으로 또 하나의 실행 스레드를 추가해 콘솔 출력 태스크를 인사말 조합 태스크와 다른 스레드에서 실행되도록 변경할 수 있습니다.

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

## 클라이언트/서버

이제 응용 프로그램 실행을 클라이언트/서버 구조로 분산하는 것도 어려운 일은 아닙니다. 변경이 필요한 부분은 배치와 관련된 코드일 뿐, 처음 작성했던 논리 케이스들은 여전히 아무 변경 없이 재사용될 수 있다는 것에 유의하세요.

[[TCP 소켓 클라이언트 서버|ko_TCP Socket Client Server]]에서 보다 자세한 내용을 다룹니다.

### Hello 서버

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

### Hello 클라이언트

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
