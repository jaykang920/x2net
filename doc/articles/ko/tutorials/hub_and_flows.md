# 허브와 플로우

x2net의 이벤트 처리는 허브에 붙은 플로우들이 시작되면서 시작되고 그 플로우들이 종료되면서 함께 종료되므로, 허브와 플로우는 x2net 응용 프로그램의 윤곽을 구성하는 핵심 구성 요소입니다.

## 허브

모든 x2net 프로세스는 하나의 허브를 중심으로 실행됩니다. x2net에서 `Hub`는 싱글턴으로 구현되므로 (`.Instance` 정적 속성으로 접근 가능한) 직접 인스턴스화할 필요는 없습니다. x2net 이벤트 처리를 중심으로 하는 전용 프로세스든 아니면 기존 프로세스에 x2net을 가볍게 덧붙인 것이든 간에 상관없이, x2net을 이용하는 모든 프로그램은 플로우들을 허브에 붙여 시작하고 종료하는 기본적인 구성을 필요로 합니다.

다음 코드 조각은 두 개의 플로우를 생성해 허브에 붙이고 시작/종료하는 기본적인 예제를 보여줍니다.

```csharp
    Hub.Instance
        .Attach(new SingleThreadFlow())  // returns Hub instance for method call chaining
        .Attach(new MultiThreadFlow());

    Hub.Startup();  // start up attached flows

    // do something...

    Hub.Shutdown();  // shut down attached flows
```

시작과 종료 코드가 굳이 분리되어야 할 필요가 없다면, 다음과 같이 `Hub.Flows` 유틸리티 객체를 이용해 `using` 블록으로 종료 호출을 자동화할 수 있습니다.

```csharp
    ...

    using (new Hub.Flows().Startup())  // start up attached flows
    {
        // do something...

        // attached flows are shut down implicitly on block exit
    }
```

## 플로우

x2net의 일반적인 플로우 구현들은 이벤트를 대기했다가 처리하는지 아니면 주기적으로 업데이트를 실행하고 처리할 이벤트를 확인하는지에 따라 크게 `EventBasedFlow`와 `FrameBasedFlow` 두 추상 클래스 중 하나로부터 상속 받게 됩니다.

`FrameBasedFlow`의 대표적인 하위 클래스는 시간 지연된 이벤트 또는 주기적으로 포스팅되는 이벤트들을 생성하는 `TimeFlow`입니다. `TimeFlow`에 대해서는 별도로 다루기로 합니다.

x2net은 `EventBasedFlow`의 하위 클래스로 스레딩 모델에 따라 다음 세 가지 유형의 플로우들을 제공합니다.

* `SingleThreadFlow` : 자체적인 단일 스레드 실행
* `MultiThreadFlow` : 자체적인 다중 스레드 실행
* `ThreadlessFlow` : 자체적인 실행 스레드 없음. 주로 처리 논리가 주 스레드에서 실행될 것을 요구하는 GUI 프로그램이나 게임 클라이언트 같은 응용 프로그램에 x2net을 끼워 넣기위해 사용됨

여러분은 이 내장 플로우 클래스들을 직접 사용하거나, 이들을 상속 받는 자신만의 플로우 클래스를 만들 수도 있습니다. 하지만 플로우 클래스를 상속 받는 것은 기존 클래스들이 지원하지 않는 새로운 동작이 필요한 경우로 한정하는 것이 좋습니다. 단순히 응용 프로그램 논리를 추가하기 위해서라면 플로우를 상속 받는 것보다 `Case`들을 구성해 기본 플로우에 추가하는 것이 더 좋은 방법입니다.

## 케이스

보통 여러분 자신의 응용 프로그램 논리를 작성하기 위해 다음과 같이 `Case` 클래스를 상속 받아 `Setup`과 `Teardown` 두 개의 메소드를 오버라아드 해 초기화와 종료 코드를 추가합니다.

```csharp
public class MyCase : Case
{
    protected override void Setup()
    {
        // 필요한 초기화, 특히 시작 이벤트-핸들러 바인딩
    }

    protected override void Teardown()
    {
        // 이벤트-핸들러 언바인딩 이외의 다른 종료 코드가 필요한 경우 작성
    }
}
```

일단 사용할 케이스들이 준비되면, 다음과 같이 허브에 붙는 플로우에 추가할 수 있습니다.

```csharp
    Hub.Instance
        .Attach(new SingleThreadFlow()
            .Add(new MyCase())  // 메소드 호출 체인을 지원하기 위해 Flow 인스턴스 리턴
            .Add(new OtherCase()))
        .Attach(new MultiThreadFlow());

    ...
```

## 허브/플로우의 시작과 종료 과정

플로우와 케이스의 초기 구성이 완료된 후 `Hub.Startup()` 메소드를 호출하면, 허브에 붙은 순서에 따라 각 플로우의 `Startup()` 메소드가 차례대로 호출됩니다. 각각의 플로우의 `Startup()` 메소드의 구현은 플로우의 종류에 따라 다르지만, 보통 다음과 같은 순서로 이루어집니다.

1. 플로우의 `protected void Setup()` 메소드가 호출됩니다.
2. 해당 플로우에 추가된 순서에 따라 각 케이스의 `protected void Setup()` 메소드가 호출됩니다. 여러분이 `Case`를 상속 받으며 재정의한 `Setup()` 메소드는 이 때 호출됩니다.
3. 플로우의 종류에 따라 자체적인 실행 스레드들을 시작하는 등의 초기화 작업이 수행됩니다.
4. 해당 플로우의 이벤트 큐에만 `FlowStart` 이벤트가 큐잉됩니다.
5. 플로우의 이벤트 처리 루틴이 처음 시작될 때 위에서 큐잉된 `FlowStart`이벤트를 첫 번째로 꺼내 처리하게 되는데, 이 때 플로우의 실행 컨텍스트 내에서 `Flow`의 `protected void OnStart()` 메소드가 호출됩니다.

위의 과정에서 여러붙이 응용 프로그램 논리 초기화를 위해 재정의할 수 있는 메소드는 다음 세 가지입니다.

1. `Flow` 클래스의 `protected void Setup()` 메소드
2. `Case` 클래스의 `protected void Setup()` 메소드
3. `Flow` 클래스의 `protected void OnStart()` 메소드
4. `Case` 클래스의 `protected void OnStart()` 메소드

이 중 1, 2는 보통 주 스레드에서 실행되고, 3은 해당 플로우의 자체적인 실행 컨텍스트 내에서 실행됩니다. 일반적으로 `Flow`를 상속 받는 것보다 `Case`를 상속 받아 추가하는 것이 권장되므로, 아마도 여러분은 2번 항목을 가장 많이 사용하게 될 것입니다.

응용 프로그램은 종료할 때 `Hub.Shutdown()` 메소드를 호출하면, 허브에 붙은 순서의 역순으로 각 플로우의 `Shutdown()` 메소드가 차례대로 호출됩니다. 각각의 플로우의 `Shutdown()` 메소드의 구현은 플로우의 종류에 따라 다르지만, 보통 다음과 같은 순서로 이루어집니다.

1. 해당 플로우의 이벤트 큐에 `FlowStop` 이벤트가 큐잉됩니다.
2. 플로우의 실행 컨텍스트가 종료되기 전에 플로우의 이벤트 처리 루틴은 위에서 큐잉된 `FlowStop` 이벤트를 마지막으로 처리할 기회를 갖습니다. 이 때 플로우의 실행 컨텍스트 내에서 `Flow`의 `protected void OnStop()` 메소드가 호출됩니다.
3. 플로우의 종류에 따라 자체적인 실행 스레드들을 종료하는 등의 정리 작업이 수행됩니다.
4. 해당 플로우에 추가된 순서의 역순으로 각 케이스의 `Teardown` 메소드가 호출됩니다. 여러분이 `Case`를 상속 받으며 재정의한 `Teardown()` 메소드가 있다면 이 때 호출됩니다.
5. 플로우의 `protected void Teardown()` 메소드가 호출됩니다.

위의 과정에서 여러붙이 응용 프로그램의 정리 작업을 위해 재정의할 수 있는 메소드는 다음 세 가지입니다.

1. `Case` 클래스의 `protected void OnStop()` 메소드
2. `Flow` 클래스의 `protected void OnStop()` 메소드
3. `Case` 클래스의 `protected void Teardown()` 메소드
4. `Flow` 클래스의 `protected void Teardown()` 메소드

이 중 1은 해당 플로우의 자체적인 실행 컨텍스트 내에서 실행되고, 2와 3은 보통 주 스레드에서 실행됩니다. 일반적으로 `Flow`를 상속 받는 것보다 `Case`를 상속 받아 추가하는 것이 권장되므로, 아마도 여러분은 2번 항목을 가장 많이 사용하게 될 것입니다.
