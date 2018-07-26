## TimeFlow and Heartbeat Event

### TimeFlow

`TimeFlow` is a utility flow that posts time-deferred or periodic events to the application hub. You can acquire the singleton instance of the default time flow through the static property `TimeFlow.Default`.

#### Reserving Deferred Events

You can simply reserve an event to be posted after a given seconds as follows:

```csharp
    // Reserve MyEvent after 10 seconds
    TimeFlow.Default.Reserve(new MyEvent(), 10.0);
```

Or you may use the `TimeSpan` struct to achieve the same.

```csharp
    // Reserve MyEvent after 10 seconds
    TimeFlow.Default.Reserve(new MyEvent(), new TimeSpan(0, 0, 10));
```

If you want to reserve an event at a absolute time, you can use the methods that accept `DateTime` struct arguemtns.

```csharp
    TimeFlow.Default.ReserveAtLocalTime(new MyEvent(), DateTime.Now + 1);

    TimeFlow.Default.ReserveAtUniversalTime(new MyEvent(), DateTime.UtcNow + 1);
```

All these reservation methods returns `Timer.Token` struct values. In order to cancel a reserved event, you can call the `Cancel` method with the corresponding token.

```csharp
    Timer.Token token = TimeFlow.Default.Reserver(new MyEvent(), 10);
    ...
    TimeFlow.Default.Cancel(token);
```

#### Reserving Periodic Events

```csharp
    // Reserve MyEvent every 10 seconds
    TimeFlow.Default.ReserveRepetition(new MyEvent(), new TimeSpan(0, 0, 10));

    // Reserve MyEvent after 1 minute, then every 10 seconds
    TimeFlow.Default.ReserveRepetition(new MyEvent(),
        DateTime.UtcNow.AddMinutes(1), new TimeSpan(0, 0, 10));
```

You can call the `CancelRepetition` method to cancel a periodic event.

```csharp
    TimeFlow.Default.CancelRepetition(new MyEvent());
```

### Heartbeat Event

When `Hub.Startup()` is called, x2net automatically reserves the periodic heartbeat event on the default time flow. You may override `protected void OnHeartbeat()` method of the `Flow` class to define what you want to do periodically. Or you can add a handler binding into your case to handle the heartbeat event as follows:

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

The default heartbeat interval is 5 seconds. You may change the heartbeat interval by assigning `Config.HeartbeatInterval` property in seconds, before calling `Hub.Startup()`. But beware of the side effects: changing the heartbeat interval will affect the Keepalive interval of builtin TCP links and other internals.
