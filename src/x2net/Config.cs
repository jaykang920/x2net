// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
#if XML_CONFIG
using System.Configuration;
#endif

namespace x2net
{
    /// <summary>
    /// Provides the global configuration properties.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Gets or sets the minimum log level.
        /// </summary>
        public static TraceLevel TraceLevel { get; set;}

        /// <summary>
        /// Gets or sets the time interval, in seconds, of built-in heartbeat
        /// events.
        /// </summary>
        public static int HeartbeatInterval { get; set; }

        public static class Flow
        {
            public static class Logging
            {
                public static class SlowHandler
                {
                    static SlowHandler()
                    {
                        // Default values
                        TraceLevel = TraceLevel.Warning;
                        Threshold = 100;  // in milliseconds
                    }

                    public static TraceLevel TraceLevel { get; set; }
                    public static int Threshold { get; set; }
                }

                public static class SlowScope
                {
                    static SlowScope()
                    {
                        // Default values
                        TraceLevel = TraceLevel.Warning;
                        Threshold = 200;  // in milliseconds
                    }

                    public static TraceLevel TraceLevel { get; set; }
                    public static int Threshold { get; set; }
                }

                public static class LongQueue
                {
                    static LongQueue()
                    {
                        // Default values
                        TraceLevel = TraceLevel.Error;
                        Threshold = 1000;
                    }

                    public static TraceLevel TraceLevel { get; set; }
                    public static int Threshold { get; set; }
                }
            }
        }

        public static int MaxLinkHandles { get; set; }

        public static class Buffer
        {
            public static class SizeExponent
            {
                static SizeExponent()
                {
                    // Default values
                    // SizeExponent.Chunk >= SizeExponent.Segment
                    Chunk = 24;  // 16MB
                    Segment = 12;  // 4KB
                }

                /// <summary>
                /// Gets or sets the buffer chunk size exponent n in 2^n.
                /// </summary>
                public static int Chunk { get; set; }
                /// <summary>
                /// Gets or sets the buffer segment size exponent n in 2^n.
                /// </summary>
                public static int Segment { get; set; }
            }

            /// <summary>
            /// Gets the buffer chunk size in bytes.
            /// </summary>
            public static int ChunkSize
            {
                get { return (1 << SizeExponent.Chunk); }
            }
            /// <summary>
            /// Gets the buffer segment size in bytes.
            /// </summary>
            public static int SegmentSize
            {
                get { return (1 << SizeExponent.Segment); }
            }

            public static class RoomFactor
            {
                static RoomFactor()
                {
                    // Default values
                    MinLevel = 0;  // x1
                    MaxLevel = 3;  // x8
                }

                public static int MinLevel { get; set; }
                public static int MaxLevel { get; set; }
            }
        }

        public static class Coroutine
        {
            static Coroutine()
            {
                // Default values
                MaxWaitHandles = 32768;
                DefaultTimeout = 30.0;  // in seconds
            }

            /// <summary>
            /// Gets or sets the maximum number of wait handles.
            /// </summary>
            public static int MaxWaitHandles { get; set; }
            /// <summary>
            /// Gets or sets the default wait timeout in seconds.
            /// </summary>
            public static double DefaultTimeout { get; set; }
        }

        static Config()
        {
            // Default values
            TraceLevel = TraceLevel.Info;
            HeartbeatInterval = 5;  // in seconds
            MaxLinkHandles = 65536;
        }

#if XML_CONFIG
        /// <summary>
        /// Loads the configuration properties from the application
        /// configuration.
        /// </summary>
        public static void Load()
        {
            ConfigSection section = (ConfigSection)
                ConfigurationManager.GetSection("x2net");

            TraceLevel = section.Trace.Level;
            HeartbeatInterval = section.Heartbeat.Interval;

            FlowLoggingElement logging = section.Flow.Logging;
            Flow.Logging.SlowHandler.TraceLevel = logging.SlowHandler.TraceLevel;
            Flow.Logging.SlowHandler.Threshold = logging.SlowHandler.Threshold;
            Flow.Logging.LongQueue.TraceLevel = logging.LongQueue.TraceLevel;
            Flow.Logging.LongQueue.Threshold = logging.LongQueue.Threshold;

            MaxLinkHandles = section.Link.MaxHandles;

            BufferElement buffer = section.Buffer;
            Buffer.SizeExponent.Chunk = buffer.SizeExponent.Chunk;
            Buffer.SizeExponent.Segment = buffer.SizeExponent.Segment;
            Buffer.RoomFactor.MinLevel = buffer.RoomFactor.MinLevel;
            Buffer.RoomFactor.MaxLevel = buffer.RoomFactor.MaxLevel;

            Coroutine.MaxWaitHandles = section.Coroutine.MaxWaitHandles;
            Coroutine.DefaultTimeout = section.Coroutine.DefaultTimeout;
        }
#endif
    }
}
