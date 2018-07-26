## Event Handlers

### Writing Event Handlers

x2net event handlers are internally .NET framework delegates. When you use an instance method as a event handler, the method does not have to be `public`. You neither have to type-cast the event argument of a handler, since it can be the same type as what you bind your handler with. The following example shows a custom case with an instance method handling `Foo` events.

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

An event handler should not change the properties of the event passed in. When an event is posted to the hub, all the flows attached to the hub share its reference, and there is no way to tell when the event will be processed in each flow. **Once an event is posted to the hub, you should treat it as an immutable object.** For example, let's say that one event was queued into two separate flows and one of the flows made a change to a specific property of the event. When the other flow is ready to handle the event, we cannot be sure whether the property has a value before or after the change.

### Binding Event Handlers

In x2net, each flow manages its own map of event-hander bindings. In order to do something meaningful upon the events that is fed to a flow, you need to add some event-handler bindings within the execution context of the flow. Although x2net encourages frequent handler binding/unbinding on precisely configured target events, you can still rely on the old traditional approach with global event handler switching on event types.

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

Just like the case of event posting, event extension methods are provided to support the following convenient usage.

```csharp
    new Event().Bind(OnEvnet);
```

The `Flow` class has a thread-static property `CurrentFlow` that identifies the flow executing the current thread. That's why you can simply call `Flow.Bind()` static method to register a binding to the current flow.

In order to remove a previously installed binding, you just call `Unbind` methods instead of `Bind`.

```csharp
    Flow.Unbind(new Event(), OnEvent);  // or new Event().Unbind(OnEvnet);
```

### Event Hierarchy and Handler Binding

When event handlers are selected for an event, the inheritance hierarchy of the event is reverse-scanned to find handler bindings for each event.

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

In the above example, `Bar` event inherits from `Foo` event and both events have handler bindings. Handling 'Foo' event instance, only `OnFoo` handler is called. Handling `Bar` event instance, both `OnBar` and `OnFoo` handlers are called.

In the first example, the handler `OnEvent` could receive all the events since the `Event` class is the common base class for all custom event types.

### Precise Event Dispatching

In x2net, you may have a handler binding that would be called exactly when some event properties match desired values. In the example below, `OnFoo` handler is called on a `Foo` event whose `Bar` property value is exactly 1. The handler is not called on a `Foo` event if its `Bar` property value is not 1.

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
