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
        private readonly Coroutine coroutine;
        private readonly Binding.Token token;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token timerToken;
        private readonly CancellationTokenSource cts;

        public WaitForTask(Coroutine coroutine, Task task)
            : this(coroutine, task, Config.Coroutine.DefaultTimeout)
        {
        }

        public WaitForTask(Coroutine coroutine, Task task, double seconds)
        {
            this.coroutine = coroutine;

            TimeoutEvent e = new TimeoutEvent { Key = task };
            token = Flow.Bind(e, OnResult);

            TimeoutEvent timeout = new TimeoutEvent { Key = this };
            timeoutToken = Flow.Bind(timeout, OnTimeout);
            timerToken = TimeFlow.Default.Reserve(timeout, seconds);

            cts = new CancellationTokenSource();
            Task.Factory.StartNew(async () => {
                await task.ConfigureAwait(false);
                Hub.Post(e);
                TimeFlow.Default.Cancel(timerToken);
            }, cts.Token);
        }

        void OnResult(TimeoutEvent e)
        {
            Flow.Unbind(token);
            Flow.Unbind(timeoutToken);

            var task = (Task)e.Key;
            coroutine.Result = null;
            coroutine.Continue();
        }

        void OnTimeout(TimeoutEvent e)
        {
            cts.Cancel();

            Flow.Unbind(token);
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
        private readonly Coroutine coroutine;
        private readonly Binding.Token token;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token timerToken;
        private readonly CancellationTokenSource cts;

        public WaitForTask(Coroutine coroutine, Task<T> task)
            : this(coroutine, task, Config.Coroutine.DefaultTimeout)
        {
        }

        public WaitForTask(Coroutine coroutine, Task<T> task, double seconds)
        {
            this.coroutine = coroutine;

            TimeoutEvent e = new TimeoutEvent { Key = task };
            token = Flow.Bind(e, OnResult);

            TimeoutEvent timeout = new TimeoutEvent { Key = this };
            timeoutToken = Flow.Bind(timeout, OnTimeout);
            timerToken = TimeFlow.Default.Reserve(timeout, seconds);

            cts = new CancellationTokenSource();
            Task.Factory.StartNew(async () => {
                await task.ConfigureAwait(false);
                Hub.Post(e);
                TimeFlow.Default.Cancel(timerToken);
            }, cts.Token);
        }

        void OnResult(TimeoutEvent e)
        {
            Flow.Unbind(token);
            Flow.Unbind(timeoutToken);

            var task = (Task<T>)e.Key;
            coroutine.Result = task.Result;
            coroutine.Continue();
        }

        void OnTimeout(TimeoutEvent e)
        {
            cts.Cancel();

            Flow.Unbind(token);
            Flow.Unbind(timeoutToken);

            coroutine.Status = CoroutineStatus.Timeout;
            coroutine.Result = null;  // indicates timeout
            coroutine.Continue();
        }
    }
}

#endif