// Copyright (c) 2017-2019 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// Binary wire format serializer.
    /// </summary>
    public sealed partial class Serializer
    {
        private Buffer buffer;

        /// <summary>
        /// Initializes a new Serializer object that works on the specified
        /// buffer.
        /// </summary>
        public Serializer(Buffer buffer)
        {
            this.buffer = buffer;
        }
    }
}
