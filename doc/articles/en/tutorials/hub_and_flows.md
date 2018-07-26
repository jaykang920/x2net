# Hub and Flows

Hub and flows are the core constructs that form the outline of x2net applications, where x2net event processing begins when the flows attached to the hub start up, and ends when those flows shut down.

## Hub

Every x2net process runs around a single hub. Since the `Hub` class is implemented as a singleton (accessable through the `.Instance` static property), you don't have to explicitly instantiate it. Regardless whether it is a dedicated application that fully relies on x2net event processing or it is an existing application that lightly adopts x2net, any x2net program requires some common code that attaches flows to the hub, starts and stops them.

The following code snippet shows how to create two flow instances, attach them to the hub, and start/stop them.

```csharp
    Hub.Instance
        .Attach(new SingleThreadFlow())  // returns Hub instance for method call chaining
        .Attach(new MultiThreadFlow());

    Hub.Startup();  // start up attached flows

    // do something...

    Hub.Shutdown();  // shut down attached flows
```

If there is no need to separate the startup and shutdown parts, you can just take advantage of the `Hub.Flows` utility object to use a `using` block around it to automate shutdown call on block exit.

```csharp
    ...

    using (new Hub.Flows().Startup())  // start up attached flows
    {
        // do something...

        // attached flows are shut down implicitly on block exit
    }
```

## Flows

Concrete flows of x2net inherit from one of the two abstract classes, `EventBasedFlow` and `FrameBasedFlow`, according to whether they wait for an event or they run periodic updates and check for any event to process, respectively.

One subclass of `FrameBasedFlow` is the `TimeFlow` class, which generates time-deferred or periodic events. We will look into the `TimeFlow` class in detail somewhere else.

x2net provides the following three subtypes of `EventBasedFlow`, according to their threading models:

* `SingleThreadFlow` : Owns a single execution thread
* `MultiThreadFlow` : Owns multiple execution threads
* `ThreadlessFlow` : No own execution thread. Mainly used to embed x2net into an application which demands that the logic processing must occur in its main thread (like a GUI program or a game client).

You may use these builtin flow classes directly, or you may write your own flows that inherit from them. But you would better restrict your flow subclassing to the cases that you need some behaviors that are not supported by existing flows. Simply adding application logic, it is a better idea to configure `Case` subclasses and add them to basic flows, than to extend existing flows.

## Case

Usually, writing your own application logic begins with subclassing the `Case` class and overriding the methods `Setup()` and `Teardown()` to define startup/cleanup routines, respectively.

```csharp
public class MyCase : Case
{
    protected override void Setup()
    {
        // Initialization code, especially initial event-handler binding
    }

    protected override void Teardown()
    {
        // Cleanup code, except event-handler unbinding, if required
    }
}
```

Once you got the cases you need, you can add them to any flow attached to the hub.

```csharp
    Hub.Instance
        .Attach(new SingleThreadFlow()
            .Add(new MyCase())  // returns Flow instance for method call chaining
            .Add(new OtherCase()))
        .Attach(new MultiThreadFlow());

    ...
```

## Analyzing Startup/Shutdown of Hub/Flows

When you call the `Hub.Startup()` method after initially configuring flows and cases, the `Startup()` method of each flow is called in the order of attachment. Builtin flows have their own `Startup()` methods, normally implemented as the following sequence:

1. `protected void Setup()` method of the flow is called.
2. `protected void Setup()` method of each case is called in the order of addition. If you overrode that method in your subclass of `Case`, the overridden method is called.
3. Initialization of execution context (such as starting threads) is performed according to the type of flow.
4. A `FlowStart` event is put into the flow's event queue.
5. When the event processing routine of the flow starts, it receives the above `FlowStart` as its first event, and the `protected void OnStart()` method of `Flow` is called on this event.

In the above sequence, there are three methods you may override to initialize your application logic:

1. `protected void Setup()` of `Flow`
2. `protected void Setup()` of `Case`
3. `protected void OnStart()` of `Flow`
4. `protected void OnStart()` of `Case`

1 and 2 are called usually in the main thread, while 3 is called in the flow's own execution context. Since subclassing `Case` is preferred over subclassing `Flow`, you will mostly use 2.

When you call the `Hub.Shutdown()` method to terminate, the `Shutdown()` method of each flow is called in the reverse order of attachment. Builtin flows have their own `Shutdown()` methods, normally implemented as the following sequence:

1. A `FlowStop` event is put into the flow's event queue.
2. Before the event processing routine of the flow terminates, it receives the above `FlowStop` as its last event, and the `protected void OnStop()` method of `Flow` is called on this event.
3. Termination of execution context (such as stopping threads) is performed according to the type of flow.
4. `protected void Teardown()` method of each case is called in the reverse order of addition. If you overrode that method in your subclass of `Case`, the overridden method is called.
5. `protected void Teardown()` method of the flow is called.

In the above sequence, there are three methods you may override to terminate your application logic:

1. `protected void OnStop()` of `Case`
2. `protected void OnStop()` of `Flow`
3. `protected void Teardown()` of `Case`
4. `protected void Teardown()` of `Flow`

1 is called in the flow's own execution context, while 2 and 3 are called usually in the main thread. Since subclassing `Case` is preferred over subclassing `Flow`, you will mostly use 2.

