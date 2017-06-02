// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// Utility struct for hash code generation.
    /// </summary>
    /// <remarks>Be aware that this struct is mutable.</remarks>
    public partial struct Hash
    {
        /// <summary>
        /// Default hash seed value.
        /// </summary>
        public const int Seed = 17;

        /// <summary>
        /// The hash code value in this instance.
        /// </summary>
        public int Code;

        /// <summary>
        /// Initializes a new instance of the Hash structure with the specified
        /// seed value.
        /// </summary>
        public Hash(int seed)
        {
            Code = seed;
        }
    }
}
