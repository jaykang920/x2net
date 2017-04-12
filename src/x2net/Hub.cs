// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

namespace x2net
{
    /// <summary>
    /// Represents the singleton event distribution bus.
    /// </summary>
    public sealed class Hub
    {
        /// <summary>
        /// Gets the singleton instance of the hub.
        /// </summary>
        public static Hub Instance { get; private set; }

        static Hub()
        {
            Instance = new Hub();
        }

        // Private constructor to prevent explicit instantiation
        private Hub()
        {

        }
    }
}
