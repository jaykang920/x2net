## 이벤트 핸들러

### 이벤트 핸들러 작성하기

x2net의 이벤트 핸들러는 내부적으로 .NET 프레임워크의 대리자(delegate)입니다. 인스턴스 메소드를 이벤트 핸들러로 사용하는 경우, 메소드가 반드시 `public`이어야 할 필요는 없습니다. 이벤트 핸들러는 해당 핸들러가 바인딩 될 이벤트의 정확한 타입의 이벤트는 인자로 받기 때문에, 이벤트 타입을 캐스팅할 필요도 없습니다. 다음 예제는 Foo 이벤트를 처리하는 인스턴스 메소드를 갖는 사용자 케이스를 작성하는 방법을 보여줍니다.

```xml
<x2>
    <event name="Foo" id="1">
        <property name="Bar" type="int32"/>
    </event>
</x2>
```

```csharp
public class MyCase : Case
{
    protected override void Setup()
    {
        new Foo().Bind(OnFoo);
    }

    void OnFoo(Foo e)
    {
        // Do not change e.Bar here!
        ...
    }
}
```

이벤트 핸들러는 인자로 전달 받은 이벤트의 속성을 변경해서는 안됩니다. 이벤트가 허브에 포스팅 되면 해당 이벤트 참조는 허브에 붙은 모든 플로우들에 공유되며, 각 플로우가 언제 그 이벤트를 처리하게 될 지는 알 수 없습니다. **따라서 일단 허브에 포스팅 된 이벤트는 변경할 수 없는 (immutable) 객체로 간주해야 합니다.** 예를 들어 하나의 이벤트가 두 개의 서로 다른 플로우에 큐잉되고 그 중 하나의 플로우가 실행한 핸들러에서 해당 이벤트의 특정 속성을 변경했다면, 다른 플로우의 핸들러가 변경되기 전의 값을 보게 될지 변경된 후의 값을 보게 될지 알 수 없게 됩니다.

### 이벤트 핸들러 바인딩

x2net에서 각각의 플로우는 자신만의 이벤트-핸들러 바인딩 맵을 관리합니다. 플로우에 공급되는 이벤트에 대해 의미 있는 처리를 하려면 각 플로우의 실행 컨텍스트 내에서 이벤트-핸들러 바인딩을 추가해야 합니다. x2net은 대상 이벤트에 따라 정교하게 제어되는 핸들러들을 수시로 추가/제거할 것을 권장하지만, 다음과 같이 모든 이벤트를 수신해 타입 식별자에 따라 분기하는 전통적인 용법도 가능합니다.

```csharp
   Flow.Bind(new Event(), OnEvent);
```
```csharp
    void OnEvent(Event e)
    {
        switch (e.GetTypeId())
        {
            case 1:
                // do something
                break;
            case 2:
                // do other thing
                break;
            ...
        }
    }
```

허브에 이벤트를 포스팅할 때처럼, 다음과 같은 보다 간편한 용법을 위해 이벤트 확장 메소드가 제공됩니다.

```csharp
    new Event().Bind(OnEvnet);
```

`Flow` 클래스는 현재 스레드를 실행 중인 플로우를 식별할 수 있는 `CurrentFlow`라는 스레드 로컬 속성을 갖고 있습니다. 우리가 현재 플로우에 바인딩을 추가하기 위해 단순히 `Flow.Bind()` 정적 메소드를 사용할 수 있는 것은 이 때문입니다.

이미 등록된 바인딩을 제거하기 위해서는 단지 `Bind` 대신 `Unbind` 메소드를 사용하면 됩니다.

```csharp
    Flow.Unbind(new Event(), OnEvent);  // or new Event().Unbind(OnEvnet);
```

### 이벤트 계층 구조와 핸들러 바인딩

어떤 이벤트에 대해 바인딩 된 핸들러들이 실행될 때에는 이벤트의 상속 계층 구조를 거슬러 올라가며 바인딩 된 핸들러들이 모두 실행됩니다.

```xml
<x2>
    <event name="Foo" id="1">
    </event>
    <event name="Bar" id="1" base="Foo">
    </event>
</x2>
```

```csharp
        new Foo().Bind(OnFoo);
        new Bar().Bind(OnBar);
    ...

    void OnFoo(Foo e) { ... }
    void OnBar(Bar e) { ... }
```

예를 들어 위의 예제와 같이 `Foo` 이벤트를 상속 받는 `Bar` 이벤트가 있고 각각에 대한 핸들러가 바인딩 되어 있다면, `Foo` 이벤트 인스턴스가 처리될 때에는 `OnFoo`만이 호출되지만 `Bar` 이벤트 인스턴스가 처리될 때에는 `OnBar`와 `OnFoo`가 모두 호출됩니다.

처음에 살펴본 예제에서 `Event`는 모든 이벤트의 상위 클래스이기 때문에 `Event`에 대한 핸들러를 바인딩하면 모든 이벤트를 수신할 수 있었습니다.

### 정교한 이벤트 디스패칭

x2net에서는 이벤트의 하나 이상의 특정 속성이 정확히 원하는 값일 때에만 이벤트 핸들러가 호출되도록 바인딩할 수 있습니다. 예를 들어, 아래와 같은 경우에는 `Bar` 속성의 값이 정확히 1인 `Foo` 이벤트에 대해서만 `OnFoo` 핸들러가 호출되며, `Bar` 속성이 설정되지 않거나 1이 아닌 값으로 설정된 경우에는 호출되지 않습니다.

```xml
<x2>
    <event name="Foo" id="1">
        <property name="Bar" type="int32"/>
    </event>
</x2>
```

```csharp
        new Foo { Bar = 1 }.Bind(OnFoo);
    ...

    void OnFoo(Foo e)
    {
        // e.Bar == 1
        ...
    }
```

