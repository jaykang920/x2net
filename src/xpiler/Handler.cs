// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

namespace x2net.xpiler
{
    /// <summary>
    /// Document file handler interface.
    /// </summary>
    interface Handler
    {
        bool Handle(string path, out Document doc);
    }
}
