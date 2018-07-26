## Possible Memory Leak and Event Sink

x2net event handlers are based on method delegates of .NET framework. They may target either static methods or instance methods.

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

Please remember that an instance method delegate preserves a strong reference to the instance. In the above example, even if myClass instance is not referenced any more, it will not be garbage-collected as long as the associated instance method handler remains. To avoid such memory leak, you need to unbind all the instance method handlers targeting an instance that is no longer used.  This is affordable when there are not many handlers, but quickly becomes tedious and error-prone as the number of handler bindings increases.

In order to minimize the possibility of such memory leak, x2net introduces the utility class `EventSink` implementing `IDisposable` interface. If any of your classes should handle events, make it a subclass of `EventSink` and call `Dispose()` when you finish using its instance in order to remove all the instance handler bindings targeting it.

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

The `Case` abstract class that you might use as a base of your business logic already inherits from `EventSink` class, and all the handler bindings referencing a case object will be automatically removed when its holding flow stops.

However, if you need to write another object that receives and handles events in your application, you should be aware of the possibility of this kind of memory leak, and consider subclassing `EventSink` and securing `Dispose()` method call.
