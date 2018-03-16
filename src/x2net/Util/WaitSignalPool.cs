// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    public static class WaitSignalPool
    {
        private static Pool<ManualResetEvent> pooled;
        private static Dictionary<int, ManualResetEvent> active;

        static WaitSignalPool()
        {
            pooled = new Pool<ManualResetEvent>();
            active = new Dictionary<int, ManualResetEvent>();
        }

        public static ManualResetEvent Acquire(int key)
        {
            ManualResetEvent waitHandle;
            lock (active)
            {
                if (!active.TryGetValue(key, out waitHandle))
                {
                    waitHandle = pooled.Pop();
                    if ((object)waitHandle == null)
                    {
                        waitHandle = new ManualResetEvent(false);
                    }
                    active.Add(key, waitHandle);
                }
            }
            return waitHandle;
        }

        public static void Release(int key)
        {
            lock (active)
            {
                ManualResetEvent waitHandle;
                if (active.TryGetValue(key, out waitHandle))
                {
                    active.Remove(key);
                    waitHandle.Reset();
                    pooled.Push(waitHandle);
                }
            }
        }
    }
}
