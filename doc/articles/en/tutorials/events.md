## Creating and Posting Events

### Creating a New Event

You can create a new instance of any event using the object allocation statement of the language. With C# object initializer block, you can initialize some public properties along with object allocation.

```csharp
    var MyEvent1 = new MyEvent();

    var myEvent2 = new MyEvent {
        Foo = 1,
        Bar = 2
    };
```

### Posting Events

You can post up an event to the hub, with the `Post()` static method of the `Hub` class.

```csharp
    ...
    var myEvent = new MyEvent {
        Foo = 1,
        Bar = 2
    };
    ...
    Hub.Post(myEvent);
```

Once an event is posted to the hub, all the attached and running flows are notified with it.
For convenience reason, x2net also provides an event extension method to support the following usage:

```csharp
    ...
    new MyEvent {
        Foo = 1,
        Bar = 2
    }.Post();
    ...
```

Since the `Hub` class is a singleton and posting events is thread-safe, you may interact with x2net at any point of your application simply by posting events.