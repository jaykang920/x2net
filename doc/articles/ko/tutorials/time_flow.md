## 타임 플로우와 하트비트 이벤트

### TimeFlow

`TimeFlow`는 시간 지연된 이벤트 또는 반복적으로 발생하는 이벤트 들을 응용 프로그램 이벤트 허브에 포스팅하기 위해 사용되는 유틸리티 플로우입니다. `TimeFlow.Default` 정적 속성으로 기본 타임 플로우의 싱글턴 인스턴스를 얻어 사용할 수 있습니다.

#### 시간 지연된 이벤트 포스팅 예약하기

간단히 지금으로부터 몇 초 후에 이벤트가 포스팅 되기를 원한다면 다음과 같이 예약할 수 있습니다.

```csharp
    // 10초 후에 MyEvent 예약
    TimeFlow.Default.Reserve(new MyEvent(), 10.0);
```

같은 작업을 위해 `TimeSpan` 구조체를 사용할 수도 있습니다.

```csharp
    // 10초 후에 MyEvent 예약
    TimeFlow.Default.Reserve(new MyEvent(), new TimeSpan(0, 0, 10));
```

예약할 시간이 지금으로부터의 상대 시간이 아니라 절대 시간이라면 다음과 같이 `DateTime` 구조체를 사용하는 예약 함수들을 사용할 수 있습니다.

```csharp
    TimeFlow.Default.ReserveAtLocalTime(new MyEvent(), DateTime.Now + 1);

    TimeFlow.Default.ReserveAtUniversalTime(new MyEvent(), DateTime.UtcNow + 1);
```

위의 모든 예약 메소드들은 `Timer.Token` 구조체 형식을 반환합니다. 예약된 이벤트를 취소하기 위해서는 이 토큰을 인자로 `Cancel` 메소드를 호출하면 됩니다.

```csharp
    Timer.Token token = TimeFlow.Default.Reserver(new MyEvent(), 10);
    ...
    TimeFlow.Default.Cancel(token);
```

#### 반복적인 이벤트 포스팅 예약하기

```csharp
    // 지금부터 매 10초마다 MyEvent 예약
    TimeFlow.Default.ReserveRepetition(new MyEvent(), new TimeSpan(0, 0, 10));

    // 지금으로부터 1분 후, 그리고 그로부터 매 10초마다 MyEvent 예약
    TimeFlow.Default.ReserveRepetition(new MyEvent(),
        DateTime.UtcNow.AddMinutes(1), new TimeSpan(0, 0, 10));
```

이벤트 반복 예약을 취소하기 위해서는 동일한 이벤트를 인자로 `CancelRepetition`을 호출하면 됩니다.

```csharp
    TimeFlow.Default.CancelRepetition(new MyEvent());
```

### 하트비트 이벤트

하트비트 이벤트는 `Hub.Startup()`이 호출될 때 기본 타임 플로우에 자동으로 반복이 예약되어 주기적으로 포스팅 됩니다. `Flow` 클래스의 `protected void OnHeartbeat()` 메소드를 오버라이드하면 하트비트 이벤트가 발생할 때마다 실행할 작업을 지정할 수 있지만, 보통 플로우가 아닌 케이스를 상속 받을 것이기 때문에 여러분의 케이스에서 다음처럼 핸들러 바인딩을 추가할 수도 있습니다.

```csharp
public class MyCase : Case
{
    protected override void Setup()
    {
        Bind(Hub.HeartbeatEvent, OnHeartbeatEvent);
    }

    void OnHeartbeatEvent(HeartbeatEvent e)
    {
        // do something
    }
}
```

하트비트 이벤트의 주기는 x2net 서브시스템을 시작하기 전에 `Config.HeartbeatInterval` 속성에 초 단위로 지정할 수 있으며, 디폴트 값은 5초입니다. 하트비트 이벤트 주기를 변경하는 것은 내장 TCP 링크의 Keepalive 주기 등에 영향을 미칠 수 있기 때문에 신중하게 고려해서 결정해야 합니다.
