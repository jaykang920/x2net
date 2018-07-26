## Registering Retrievable Events

When your x2net application is distributed across processes and receives serialized events through network, an x2net link should be able to create a new event instance out of its event type identifier. In this case, you need to register the events you want to retrieve from serialized stream into `EventFactory`.

When there is only a few events to retrieve, you can register them to `EventFactory` with their runtime types as generic parameters:

```csharp
    EventFactory.Global.Register<MyEvent>();
```

If you have fairly many events to be retrieved, you may use the following method to register all the `Event` subtypes in a given assembly with a single call.

```csharp
    EventFactory.Global.Register(Assembly.Load("My.Shared.Assembly"));
```
