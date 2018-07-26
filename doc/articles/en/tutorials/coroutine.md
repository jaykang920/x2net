## Coroutine

x2net coroutines are implemented on top of .NET framework iterators, to support suspending and resuming execution within a single thread. x2net coroutines let you work with more intuitive code, binding a couple of sequential tasks into one handler method.

### Constraints

Just like .NET framework iterators which they are based on, x2net coroutines do not form a general-purpose coroutine library. There are some constraints using x2net coroutines.

* They can be used within x2net event handlers.
* They can be used within a single thread. So you can use them with builtin `SingleThreadFlow` and `ThreadlessFlow` (providing that the host thread is single), but not with `MultiThreadFlow`.

### Time Delay

First, we will look into the most simple one, `WaitForSeconds` that introduces a time delay into execution.

```csharp
using System.Collections;

using x2net;

public class MyCase : Case
{
    protected override void Setup()
    {
        Bind(new MyEvent(), OnMyEvent);
    }

    IEnumerator OnMyEvent(Coroutine coroutine, MyEvent e)
    {
        // now
        yield return coroutine.WaitForSeconds(10);
        // 10 seconds later
    }
}
```

In the above example, the event handler `OnMyEvent` starts, waits for 10 seconds, and resumes to finish. Please note that the execution thread doesn't block during the wait time. Once a coroutine handler returns prematurely on a `yield` statement, the execution thread continues to invoke as many next handlers as they can. After the specified seconds, the control returns to the next line of the `yield` statement and goes on.

### Waiting for Events

When you want to wait for a specific event within an event handler, you can use `WaitForEvent`. Without coroutines, you would have two separate event handlers for it.

```csharp
using System.Collections;

using x2net;

public class MyCase : Case
{
    protected override void Setup()
    {
        Bind(new MyEvent(), OnMyEvent);
    }

    IEnumerator OnMyEvent(Coroutine coroutine, MyEvent e)
    {
        // now

        yield return coroutine.WaitForEvent(new MyOtherEvent());

        // On MyOtherEvent posted, or timeout occurred
        var result = coroutine.Result as MyOtherEvent;
        if ((object)result != null)
        {
            // result is the MyOtherEvent instance posted
        }
        else
        {
            // timeout
        }
    }
}
```

Now when the control reaches the `yield` statement of the event handler `OnMyEvent`, it starts to wait for another event `MyOtherEvent` being posted. As said before, the execution thread doesn't block and continues to invoke next handlers. When the desired `MyOtherEvent` is posted or the timeout occurs, the control returns to the next line of the `yield` statement. You can see that it is possible to check for timeout and acquire the resultant event instance through the `Result` property of the `Coroutine` object.

### Posting Requests and Waiting for Responses

Designing a distributed application, one of the most frequent tasks is sending a request and receiving a response to go on. For this purpose, you can use `WaitForResponse` derived from `WaitForEvent`.

```csharp
using System.Collections;

using x2net;

public class MyCase : Case
{
    protected override void Setup()
    {
        Bind(new MyEvent(), OnMyEvent);
    }

    IEnumerator OnMyEvent(Coroutine coroutine, MyEvent e)
    {
        // now

        yield return coroutine.WaitForResponse(
            new MyReq(),
            new MyResp());

        // On MyResp posted, or timeout occurred
        var result = coroutine.Result as MyResp;
        if ((object)result != null)
        {
            // result is the MyResp instance posted
        }
        else
        {
            // timeout
        }
    }
}
```

`WaitForResponse` simply posts the request event when it starts to wait. Due to the nature of x2net, this handler has no idea about where the posted request is actually handled. However, if the request is successfully handled and the response event is posted to the hub, the control returns to the next line of `yield` statement and you can access the resultant event through the `Result` property of the `Coroutine` object.
