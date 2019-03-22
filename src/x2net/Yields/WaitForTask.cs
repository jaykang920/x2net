#if NET45

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace x2net
{
    /// <summary>
    /// YieldInstruction that waits for the specified task.
    /// </summary>
    public class WaitForTask : Yield
    {
        private static TaskFactory taskFactory;
        private readonly Binding.Token handlerToken;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        private readonly CancellationTokenSource cts;

        static WaitForTask()
        {
            taskFactory = new TaskFactory(new SimpleTaskScheduler());
        }

        public WaitForTask(Coroutine coroutine, Task task)
            : this(coroutine, task, Config.Coroutine.DefaultTimeout)
        {
        }

        public WaitForTask(Coroutine coroutine, Task task, double seconds)
            : base(coroutine)
        {
            LocalEvent e = new LocalEvent { Key = task };
            handlerToken = Flow.Bind(e, OnResult);

            // No timeout when seconds <= 0
            if (seconds > 0)
            {
                TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
                timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
                timerToken = TimeFlow.Instance.Reserve(timeoutEvent, seconds);
            }

            cts = new CancellationTokenSource();
            var flow = Flow.CurrentFlow;
            taskFactory.StartNew(async () => {
                await task.ConfigureAwait(false);
                flow.Feed(e);
            }, cts.Token);
        }

        void OnResult(LocalEvent e)
        {
            Flow.Unbind(handlerToken);

            if (timerToken.HasValue)
            {
                TimeFlow.Instance.Cancel(timerToken.Value);
                Flow.Unbind(timeoutToken);
            }

            var task = (Task)e.Key;
            if (task.Status != TaskStatus.RanToCompletion)
            {
                coroutine.Status = CoroutineStatus.Error;
            }
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
        private static TaskFactory taskFactory;
        private readonly Binding.Token handlerToken;
        private readonly Binding.Token timeoutToken;
        private readonly Timer.Token? timerToken;

        private readonly CancellationTokenSource cts;

        static WaitForTask()
        {
            taskFactory = new TaskFactory(new SimpleTaskScheduler());
        }

        public WaitForTask(Coroutine coroutine, Task<T> task)
            : this(coroutine, task, Config.Coroutine.DefaultTimeout)
        {
        }

        public WaitForTask(Coroutine coroutine, Task<T> task, double seconds)
            : base(coroutine)
        {
            LocalEvent e = new LocalEvent { Key = task };
            handlerToken = Flow.Bind(e, OnResult);

            // No timeout when seconds <= 0
            if (seconds > 0)
            {
                TimeoutEvent timeoutEvent = new TimeoutEvent { Key = this };
                timeoutToken = Flow.Bind(timeoutEvent, OnTimeout);
                timerToken = TimeFlow.Instance.Reserve(timeoutEvent, seconds);
            }

            cts = new CancellationTokenSource();
            var flow = Flow.CurrentFlow;
            taskFactory.StartNew(async () => {
                await task.ConfigureAwait(false);
                flow.Feed(e);
            }, cts.Token);
        }

        void OnResult(LocalEvent e)
        {
            Flow.Unbind(handlerToken);

            if (timerToken.HasValue)
            {
                TimeFlow.Instance.Cancel(timerToken.Value);
                Flow.Unbind(timeoutToken);
            }

            var task = (Task<T>)e.Key;
            if (task.Status == TaskStatus.RanToCompletion)
            {
                coroutine.Result = task.Result;
            }
            else
            {
                coroutine.Status = CoroutineStatus.Error;
                coroutine.Result = task;
            }
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