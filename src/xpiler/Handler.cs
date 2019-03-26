// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

namespace x2net.xpiler
{
    /// <summary>
    /// Definition unit file handler interface.
    /// </summary>
    interface Handler
    {
        bool Handle(string path, out Unit unit);
    }
}
