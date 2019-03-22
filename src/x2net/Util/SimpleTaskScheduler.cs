using System.Collections.Generic;
using System.Threading.Tasks;

namespace x2net
{
    internal sealed class SimpleTaskScheduler : TaskScheduler
    {
        private static class EmptyArray<T>
        {
            internal static readonly T[] Value = new T[0];
        }

        protected override void QueueTask(Task task)
        {
            TryExecuteTask(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            TryExecuteTask(task);
            return true;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return EmptyArray<Task>.Value;
        }

        public override int MaximumConcurrencyLevel { get { return 1; } }
    }
}
