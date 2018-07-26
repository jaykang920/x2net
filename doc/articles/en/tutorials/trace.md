## Capturing Log Messages

Communication links and other internal classes of x2net can emit their own log messages. However, in order to eliminate unnecessary dependency and to utilize prevailing better logging modules (including your own), x2net does not support out-of-box log file writing.

To capture internal log messages, you can simply register your own log message handler delegate.

```csharp
    public static int Main(string[] args)
    {
        x2net.Trace.Handler = (level, message) => Console.WriteLine(message);

        ...
    }
```

As shown in the example below, you can specify the minimum level of log messages that are passed into the log handler, assigning the `x2net.Config.TraceLevel` property with one of the `x2net.TraceLevel` enum constants. By default, messages of `TraceLevel.Info` or higher levels are emitted.

```csharp
    x2net.Config.TraceLevel = x2net.TraceLevel.Debug;
    x2net.Trace.Handler = ...
```