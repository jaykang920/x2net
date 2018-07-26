## 코루틴

x2net의 코루틴은 .NET 프레임워크의 반복자를 이용해 구현된, 단일 스레드 내에서의 실행 중단과 재개를 지원하는 장치입니다. 코루틴을 사용하지 않는 경우 둘 이상의 핸들러로 분리되어야 했던 작업을 코루틴을 사용하면 하나의 메소드로 묶을 수 있어, 보다 직관적인 개발을 가능하게 해 줍니다.

### 제약 사항

.NET 프레임워크의 반복자도 그렇듯이, x2net의 코루틴도 범용적으로 쓸 수 있는 코루틴 라이브러리는 아닙니다. x2net의 코루틴을 사용하는 데에는 다음과 같은 제약 사항들이 있습니다.

* x2net 이벤트 핸들러 내에서만 사용할 수 있습니다.
* 단일 스레드에서만 사용할 수 있습니다. 내장 플로우 중 `SingleThreadFlow` 또는 `ThreadlessFlow`의 호스트 스레드가 단일 스레드인 경우에는 사용할 수 있지만, `MultiThreadFlow`와 함께 사용할 수는 없습니다.

### 시간 지연

가장 간단한 예로 실행 중 시간 지연을 추가할 수 있는 `WaitForSeconds`의 용법을 살펴보겠습니다.

```csharp
using System.Collections;

using x2net;

public class MyCase : Case
{
    protected override void Setup()
    {
        Bind(new MyEvent(), OnMyEvent);
    }

    IEnumerator OnMyEvent(Coroutine coroutine, MyEvent e)
    {
        // 지금
        yield return coroutine.WaitForSeconds(10);
        // 10초 후
    }
}
```

이벤트 핸들러 `OnMyEvent`는 진입 후 10초가 지난 후 실행이 계속되어 종료됩니다. 코루틴의 장점은 이 지연 시간 동안 핸들러 실행 스레드자체는 중단되지 않는다는 것입니다. 코루틴 대기를 시작하면서 일단 핸들러가 리턴되면 실행 스레드는 다음 핸들러들의 처리를 계속할 수 있습니다. 설정된 시간이 지나면 제어가 `yield` 문 다음으로 되돌아와 중단되었던 처리를 계속할 수 있게 됩니다.

### 이벤트 대기

핸들러 실행 중에 특정 이벤트가 발생할 때까지 기다리고자 할 때는 `WaitForEvent`를 사용할 수 있습니다. 아래 예제의 이벤트 핸들러는 코루틴을 사용하지 않았다면 두 개로 분리될 수 밖에 없었을 것입니다.

```csharp
using System.Collections;

using x2net;

public class MyCase : Case
{
    protected override void Setup()
    {
        Bind(new MyEvent(), OnMyEvent);
    }

    IEnumerator OnMyEvent(Coroutine coroutine, MyEvent e)
    {
        // 지금

        yield return coroutine.WaitForEvent(new MyOtherEvent());

        // MyOtherEvent가 포스팅 된 후, 또는 타임아웃 시간이 경과했을 때
        var result = coroutine.Result as MyOtherEvent;
        if ((object)result != null)
        {
            // result는 발생한 MyOtherEvent 인스턴스
        }
        else
        {
            // 타임아웃
        }
    }
}
```

이벤트 핸들러 `OnMyEvent`는 이제 진입 후 `yield` 문에 이르면 `MyOtherEvent`라는 또 다른 이벤트를 기다리며 대기 상태로 들어갑니다. 앞서 이야기했듯이 이 대기 시간 동안 스레드는 다음 핸들러들을 계속 처리할 수 있습니다. 지정된 `MyOtherEvent`가 발생하거나 그렇지 않은 채로 타임아웃 시간이 경과하면, `yield` 문 다음으로 제어가 넘어옵니다. 위의 예제는 `Coroutine` 객체의 `Result` 속성으로 타임아웃 여부를 판단하고 결과 이벤트 객체를 얻을 수 있음을 보여줍니다.

### 요청을 포스팅하고 응답 대기하기

분산 처리 구조를 설계할 때 빈번히 필요로 하는 작업 중 하나는 요청을 보내고 그에 대한 응답을 받아 처리를 계속하는 것입니다. 이를 위해 위의 `WaitForEvent`에서 파생된 `WaitForResponse`를 사용할 수 있습니다.

```csharp
using System.Collections;

using x2net;

public class MyCase : Case
{
    protected override void Setup()
    {
        Bind(new MyEvent(), OnMyEvent);
    }

    IEnumerator OnMyEvent(Coroutine coroutine, MyEvent e)
    {
        // 지금

        yield return coroutine.WaitForResponse(
            new MyReq(),
            new MyResp());

        // MyResp가 포스팅 된 후, 또는 타임아웃 시간이 경과했을 때
        var result = coroutine.Result as MyResp;
        if ((object)result != null)
        {
            // result는 발생한 MyResp 인스턴스
        }
        else
        {
            // 타임아웃
        }
    }
}
```

`WaitForResponse`는 응답 이벤트 대기에 들어가기에 앞서 단순히 요청 이벤트를 포스팅 하도록 되어 있습니다. 포스팅 된 요청이 같은 프로세스에서 처리될지 네트워크 건너편의 다른 프로세스에서 처리될지에 대해서는 이 핸들러는 알지 못합니다. 어디에서든 처리가 성공해 응답 이벤트가 이 응용 프로그램 허브에 포스팅 되면, 제어는 `yield` 문 다음으로 넘어오고 `Coroutine` 객체의 `Result` 속성을 통해 결과 이벤트에 접근할 수 있습니다.