// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;
#if XML_CONFIG
using System.Configuration;

namespace x2
{
    /// <summary>
    /// x2net configuration section handler.
    /// </summary>
    public sealed class ConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("log")]
        public LogElement Log
        {
            get { return (LogElement)this["log"]; }
            set { this["log"] = value; }
        }

        [ConfigurationProperty("heartbeat")]
        public HeartbeatElement Heartbeat
        {
            get { return (HeartbeatElement)this["heartbeat"]; }
            set { this["heartbeat"] = value; }
        }

        [ConfigurationProperty("flow")]
        public FlowElement Flow
        {
            get { return (FlowElement)this["flow"]; }
            set { this["flow"] = value; }
        }

        [ConfigurationProperty("link")]
        public LinkElement Link
        {
            get { return (LinkElement)this["link"]; }
            set { this["link"] = value; }
        }

        [ConfigurationProperty("buffer")]
        public BufferElement Buffer
        {
            get { return (BufferElement)this["buffer"]; }
            set { this["buffer"] = value; }
        }

        [ConfigurationProperty("coroutine")]
        public CoroutineElement Coroutine
        {
            get { return (CoroutineElement)this["coroutine"]; }
            set { this["coroutine"] = value; }
        }
    }

    // x2net/log configuration element handler
    public class LogElement : ConfigurationElement
    {
        [ConfigurationProperty("level")]
        public LogLevel Level
        {
            get { return (LogLevel)this["level"]; }
            set { this["level"] = value; }
        }
    }

    // x2net/heartbeat configuration element handler
    public class HeartbeatElement : ConfigurationElement
    {
        [ConfigurationProperty("interval")]
        public int Interval
        {
            get { return (int)this["interval"]; }
            set { this["interval"] = value; }
        }
    }

    // x2net/flow configuration element handler
    public class FlowElement : ConfigurationElement
    {
        [ConfigurationProperty("logging")]
        public FlowLoggingElement Logging
        {
            get { return (FlowLoggingElement)this["logging"]; }
            set { this["logging"] = value; }
        }
    }

    // x2net/flow/logging configuration element handler
    public class FlowLoggingElement : ConfigurationElement
    {
        [ConfigurationProperty("slowHandler")]
        public SlowHandlerElement SlowHandler
        {
            get { return (SlowHandlerElement)this["slowHandler"]; }
            set { this["slowHandler"] = value; }
        }

        [ConfigurationProperty("longQueue")]
        public LongQueueElement LongQueue
        {
            get { return (LongQueueElement)this["longQueue"]; }
            set { this["longQueue"] = value; }
        }
    }

    // x2net/flow/logging/slowHandler configuration element handler
    public class SlowHandlerElement : ConfigurationElement
    {
        [ConfigurationProperty("logLevel")]
        public LogLevel LogLevel
        {
            get { return (LogLevel)this["logLevel"]; }
            set { this["logLevel"] = value; }
        }

        [ConfigurationProperty("threshold")]
        public int Threshold
        {
            get { return (int)this["threshold"]; }
            set { this["threshold"] = value; }
        }
    }

    // x2net/flow/logging/longQueue configuration element handler
    public class LongQueueElement : ConfigurationElement
    {
        [ConfigurationProperty("logLevel")]
        public LogLevel LogLevel
        {
            get { return (LogLevel)this["logLevel"]; }
            set { this["logLevel"] = value; }
        }

        [ConfigurationProperty("threshold")]
        public int Threshold
        {
            get { return (int)this["threshold"]; }
            set { this["threshold"] = value; }
        }
    }

    // x2net/link configuration element handler
    public class LinkElement : ConfigurationElement
    {
        [ConfigurationProperty("maxHandles")]
        public int MaxHandles
        {
            get { return (int)this["maxHandles"]; }
            set { this["maxHandles"] = value; }
        }
    }

    // x2net/buffer configuration element handler
    public class BufferElement : ConfigurationElement
    {
        [ConfigurationProperty("sizeExponent")]
        public SizeExponentElement SizeExponent
        {
            get { return (SizeExponentElement)this["sizeExponent"]; }
            set { this["sizeExponent"] = value; }
        }

        [ConfigurationProperty("roomFactor")]
        public RoomFactorElement RoomFactor
        {
            get { return (RoomFactorElement)this["roomFactor"]; }
            set { this["roomFactor"] = value; }
        }
    }

    // x2net/buffer/sizeExponent configuration element handler
    public class SizeExponentElement : ConfigurationElement
    {
        [ConfigurationProperty("chunk")]
        public int Chunk
        {
            get { return (int)this["chunk"]; }
            set { this["chunk"] = value; }
        }

        [ConfigurationProperty("segment")]
        public int Segment
        {
            get { return (int)this["segment"]; }
            set { this["segment"] = value; }
        }
    }

    // x2net/buffer/roomFactor configuration element handler
    public class RoomFactorElement : ConfigurationElement
    {
        [ConfigurationProperty("minLevel")]
        public int MinLevel
        {
            get { return (int)this["minLevel"]; }
            set { this["minLevel"] = value; }
        }

        [ConfigurationProperty("maxLevel")]
        public int MaxLevel
        {
            get { return (int)this["maxLevel"]; }
            set { this["maxLevel"] = value; }
        }
    }

    // x2net/coroutine configuration element handler
    public class CoroutineElement : ConfigurationElement
    {
        [ConfigurationProperty("maxWaitHandles")]
        public int MaxWaitHandles
        {
            get { return (int)this["maxWaitHandles"]; }
            set { this["maxWaitHandles"] = value; }
        }

        [ConfigurationProperty("defaultTimeout")]
        public double DefaultTimeout
        {
            get { return (double)this["defaultTimeout"]; }
            set { this["defaultTimeout"] = value; }
        }
    }
}
#endif
