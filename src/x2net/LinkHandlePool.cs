// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    public static class LinkHandlePool
    {
        private static RangedIntPool pool;

        static LinkHandlePool()
        {
            pool = new RangedIntPool(1, Config.MaxLinkHandles, true);
        }

        public static int Acquire()
        {
            return pool.Acquire();
        }

        public static void Release(int handle)
        {
            pool.Release(handle);
        }
    }
}
