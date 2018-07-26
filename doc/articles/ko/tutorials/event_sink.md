## 잠재적 메모리 누수와 이벤트 싱크

x2net의 이벤트 핸들러는 .NET 프레임워크의 대리자(delegate)에 기반하고 있습니다. 이벤트 핸들러로 사용되는 대리자는 특정 클래스의 정적 메소드일 수도 있고 특정 객체의 인스턴스 메소드일 수도 있습니다.

```csharp
    new Event().Bind(MyClass.StaticMethod);  // Static method delegate handler

    var myClass = new MyClass();
    new Event().Bind(myClass.InstanceMethod);  // Instance method delegate handler
```

```csharp
public class MyClass
{
    public static StaticMethod(Event e) { ... }

    public InstanceMethod(Event e) { ... }
}
```

인스턴스 메소드의 대리자는 해당 인스턴스에 대해 강한 참조를 유지하고 있다는 것을 기억하십시오. 위의 경우 myClass 인스턴스 참조가 더 이상 사용되지 않게 되더라도 바인딩 된 핸들러가 남아 있는 한 myClass 인스턴스는 가비지 컬렉션에서 제외되며 계속 살아 남아 있게 됩니다. 이런 잠재적인 메모리 누수를 방지하기 위해서는 사용이 끝난 객체의 인스턴스 메소드 대리자 핸들러들을 모두 제거해 주어야 합니다. 메소드 핸들러 수가 얼마 되지 않을 때는 가능하겠지만 동적으로 추가/제거하는 핸들러 수가 많아질수록 이 작업은 귀찮아지게 됩니다.

이런 잠재적인 메모리 누수의 가능성을 최소화하기 위해 x2net은 `IDisposable` 인터페이스를 구현하는 `EventSink`라는 유틸리티 클래스를 제공합니다. 만약 여러분의 어떤 클래스의 인스턴스가 이벤트를 받아 처리해야 한다면, 해당 클래스가 EventSink로부터 상속 받도록 하고 인스턴스의 사용이 끝나면 `Dispose()`메소드를 호출함으로써 해당 인스턴스를 참조로 갖는 모든 핸들러 바인딩들이 일괄 제거할 수 있습니다.

```csharp
    new Event().Bind(MyClass.StaticMethod);  // Static method delegate handler

    var myClass = new MyClass();
    new Event().Bind(myClass.InstanceMethod);  // Instance method delegate handler
    ...
    myClass.Dispose();  // Removes all the handler bindings targeting this instance
```

```csharp
public class MyClass : EventSink
{
    public static StaticMethod(Event e) { ... }

    public InstanceMethod(Event e) { ... }
}
```

여러분이 주로 응용 프로그램 논리 구현 기반으로 사용할 `Case` 추상 클래스는 이미 `EventSink` 클래스로부터 상속 받고 있으며, 해당 케이스가 포함된 플로우가 종료될 때 그것을 참조하는 모든 핸들러 바인딩들이 자동으로 제거되도록 구성되어 있습니다.

하지만, 여러분의 응용 프로그램에 이벤트를 수신해 처리하는 다른 객체를 작성해야 한다면, 이런 메모리 누수의 가능성을 고려해 가급적 `EventSink` 클래스로부터 상속 받고 `Dispose()` 호출을 보장해 주어야 합니다.