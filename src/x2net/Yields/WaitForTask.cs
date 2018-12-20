#if NET45

using System.Threading;
using System.Threading.Tasks;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for the specified task.
    /// </summary>
    public class WaitForTask : Yield
    {
        private readonly Binding.Token handlerToken;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        private readonly CancellationTokenSource cts;

        public WaitForTask(Coroutine coroutine, Task task)
            : this(coroutine, task, Config.Coroutine.DefaultTimeout)
        {
        }

        public WaitForTask(Coroutine coroutine, Task task, double seconds)
            : base(coroutine)
        {
            TimeoutEvent e = new TimeoutEvent { Key = task };
            handlerToken = Flow.Bind(e, OnResult);

            // No timeout when seconds <= 0
            if (seconds > 0)
            {
                TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
                timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
                timerToken = TimeFlow.Instance.Reserve(timeoutEvent, seconds);
            }

            cts = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                task.Wait();
                Hub.Post(e);
            }, cts.Token);
        }

        void OnResult(TimeoutEvent e)
        {
            Flow.Unbind(handlerToken);

            if (timerToken.HasValue)
            {
                TimeFlow.Instance.Cancel(timerToken.Value);
                Flow.Unbind(timeoutToken);
            }

            var task = (Task)e.Key;
            coroutine.Result = task;
            coroutine.Continue();
        }

        void OnTimeout(TimeoutEvent e)
        {
            cts.Cancel();

            Flow.Unbind(handlerToken);
            Flow.Unbind(timeoutToken);

            coroutine.Status = CoroutineStatus.Timeout;
            coroutine.Result = null;  // indicates timeout
            coroutine.Continue();
        }
    }

    /// <summary>
    /// YieldInstruction that waits for the specified task.
    /// </summary>
    public class WaitForTask<T> : Yield
    {
        private readonly Binding.Token handlerToken;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        private readonly CancellationTokenSource cts;

        public WaitForTask(Coroutine coroutine, Task<T> task)
            : this(coroutine, task, Config.Coroutine.DefaultTimeout)
        {
        }

        public WaitForTask(Coroutine coroutine, Task<T> task, double seconds)
            : base(coroutine)
        {
            TimeoutEvent e = new TimeoutEvent { Key = task };
            handlerToken = Flow.Bind(e, OnResult);

            // No timeout when seconds <= 0
            if (seconds > 0)
            {
                TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
                timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
                timerToken = TimeFlow.Instance.Reserve(timeoutEvent, seconds);
            }

            cts = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                task.Wait();
                Hub.Post(e);
            }, cts.Token);
        }

        void OnResult(TimeoutEvent e)
        {
            Flow.Unbind(handlerToken);

            if (timerToken.HasValue)
            {
                TimeFlow.Instance.Cancel(timerToken.Value);
                Flow.Unbind(timeoutToken);
            }

            var task = (Task<T>)e.Key;
            coroutine.Result = task.Result;
            coroutine.Continue();
        }

        void OnTimeout(TimeoutEvent e)
        {
            cts.Cancel();

            Flow.Unbind(handlerToken);
            Flow.Unbind(timeoutToken);

            coroutine.Status = CoroutineStatus.Timeout;
            coroutine.Result = null;  // indicates timeout
            coroutine.Continue();
        }
    }
}

#endif