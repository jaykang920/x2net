using System;
using System.Threading.Tasks;

using x2net;

namespace x2net
{
    /// <summary>
    /// Supports the interaction between an outside thread and the x2 subsystem.
    /// </summary>
    public static class EventBridge
    {
        [ThreadStatic]
        private static ThreadlessFlow flow;  // thread-local flow instance.

        public static T Wait<T>(double timeoutInSeconds = 60)
            where T : Event, new()
        {
            return Wait<T>(null, timeoutInSeconds);
        }

        public static T Wait<T>(Event req, double timeoutInSeconds = 60)
            where T : Event, new()
        {
            if (ReferenceEquals(flow, null))
            {
                int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
                flow = new ThreadlessFlow(String.Format("EventBridge-{0}", id));
                flow.Start();
            }
            flow.Attach();
            int waitHandle = WaitHandlePool.Acquire();
            try
            {
                if (!(req is null))
                {
                    req._WaitHandle = waitHandle;
                    req.Post();
                }

                T resp = null;
                flow.Wait(new T { _WaitHandle = waitHandle }, out resp, timeoutInSeconds);
                return resp;
            }
            finally
            {
                WaitHandlePool.Release(waitHandle);
                flow.Detach();
            }
        }

        public static async Task<T> WaitAsync<T>(double timeoutInSeconds = 60)
            where T : Event, new()
        {
            return await WaitAsync<T>(null, timeoutInSeconds);
        }

        public static async Task<T> WaitAsync<T>(Event req, double timeoutInSeconds = 60)
            where T : Event, new()
        {
            return await Task.Factory.StartNew(() => {
                return Wait<T>(req, timeoutInSeconds);
            });
        }
    }
}
