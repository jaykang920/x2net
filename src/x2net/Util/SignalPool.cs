// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2net
{
    public static class SignalPool
    {
        private static Pool<ManualResetEvent> pooled;
        private static Dictionary<int, ManualResetEvent> active;

        static SignalPool()
        {
            pooled = new Pool<ManualResetEvent>();
            active = new Dictionary<int, ManualResetEvent>();
        }

        public static ManualResetEvent Acquire(int key)
        {
            ManualResetEvent signal;
            lock (active)
            {
                if (!active.TryGetValue(key, out signal))
                {
                    signal = pooled.Pop();
                    if ((object)signal == null)
                    {
                        signal = new ManualResetEvent(false);
                    }
                    active.Add(key, signal);
                }
            }
            return signal;
        }

        public static void Release(int key)
        {
            lock (active)
            {
                ManualResetEvent signal;
                if (active.TryGetValue(key, out signal))
                {
                    active.Remove(key);
                    signal.Reset();
                    pooled.Push(signal);
                }
            }
        }
    }
}
