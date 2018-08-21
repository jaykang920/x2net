// Copyright (c) 2017, 2018 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// Specifies the trace level.
    /// </summary>
    public enum TraceLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        None,
    }

    /// <summary>
    /// Represents the method that handles trace calls.
    /// </summary>
    public delegate void TraceHandler(TraceLevel level, string message);

    /// <summary>
    /// Represents the tracing helper class.
    /// </summary>
    public static class Trace
    {
        /// <summary>
        /// Gets or sets the application-provided trace handler.
        /// </summary>
        public static TraceHandler Handler { get; set; }

        public static void Emit(TraceLevel level, string message)
        {
            if (Handler == null || Config.TraceLevel > level)
            {
                return;
            }
            Handler(level, message);
        }

        public static void Emit(TraceLevel level, string format, object arg0)
        {
            if ((Handler == null) || Config.TraceLevel > level)
            {
                return;
            }
            Handler(level, String.Format(format, arg0));
        }

        public static void Emit(TraceLevel level, string format, object arg0, object arg1)
        {
            if ((Handler == null) || Config.TraceLevel > level)
            {
                return;
            }
            Handler(level, String.Format(format, arg0, arg1));
        }

        public static void Emit(TraceLevel level, string format, object arg0, object arg1, object arg2)
        {
            if ((Handler == null) || Config.TraceLevel > level)
            {
                return;
            }
            Handler(level, String.Format(format, arg0, arg1, arg2));
        }

        public static void Emit(TraceLevel level, string format, params object[] args)
        {
            if ((Handler == null) || Config.TraceLevel > level)
            {
                return;
            }
            Handler(level, String.Format(format, args));
        }

        #region Trace

        public static void Log(string message)
        {
            Emit(TraceLevel.Trace, message);
        }

        public static void Log(string format, object arg0)
        {
            Emit(TraceLevel.Trace, format, arg0);
        }

        public static void Log(string format, object arg0, object arg1)
        {
            Emit(TraceLevel.Trace, format, arg0, arg1);
        }

        public static void Log(string format, object arg0, object arg1, object arg2)
        {
            Emit(TraceLevel.Trace, format, arg0, arg1, arg2);
        }

        public static void Log(string format, params object[] args)
        {
            Emit(TraceLevel.Trace, format, args);
        }

        #endregion

        #region Debug

        public static void Debug(string message)
        {
            Emit(TraceLevel.Debug, message);
        }

        public static void Debug(string format, object arg0)
        {
            Emit(TraceLevel.Debug, format, arg0);
        }

        public static void Debug(string format, object arg0, object arg1)
        {
            Emit(TraceLevel.Debug, format, arg0, arg1);
        }

        public static void Debug(string format, object arg0, object arg1, object arg2)
        {
            Emit(TraceLevel.Debug, format, arg0, arg1, arg2);
        }

        public static void Debug(string format, params object[] args)
        {
            Emit(TraceLevel.Debug, format, args);
        }

        #endregion

        #region Info

        public static void Info(string message)
        {
            Emit(TraceLevel.Info, message);
        }

        public static void Info(string format, object arg0)
        {
            Emit(TraceLevel.Info, format, arg0);
        }

        public static void Info(string format, object arg0, object arg1)
        {
            Emit(TraceLevel.Info, format, arg0, arg1);
        }

        public static void Info(string format, object arg0, object arg1, object arg2)
        {
            Emit(TraceLevel.Info, format, arg0, arg1, arg2);
        }

        public static void Info(string format, params object[] args)
        {
            Emit(TraceLevel.Info, format, args);
        }

        #endregion

        #region Warn

        public static void Warn(string message)
        {
            Emit(TraceLevel.Warning, message);
        }

        public static void Warn(string format, object arg0)
        {
            Emit(TraceLevel.Warning, format, arg0);
        }

        public static void Warn(string format, object arg0, object arg1)
        {
            Emit(TraceLevel.Warning, format, arg0, arg1);
        }

        public static void Warn(string format, object arg0, object arg1, object arg2)
        {
            Emit(TraceLevel.Warning, format, arg0, arg1, arg2);
        }

        public static void Warn(string format, params object[] args)
        {
            Emit(TraceLevel.Warning, format, args);
        }

        #endregion

        #region Error

        public static void Error(string message)
        {
            Emit(TraceLevel.Error, message);
        }

        public static void Error(string message, Exception exception)
        {
            Emit(TraceLevel.Error, String.Format("{0} {1}", message, exception.ToString()));
        }

        public static void Error(string format, object arg0)
        {
            Emit(TraceLevel.Error, format, arg0);
        }

        public static void Error(string format, object arg0, object arg1)
        {
            Emit(TraceLevel.Error, format, arg0, arg1);
        }

        public static void Error(string format, object arg0, object arg1, object arg2)
        {
            Emit(TraceLevel.Error, format, arg0, arg1, arg2);
        }

        public static void Error(string format, params object[] args)
        {
            Emit(TraceLevel.Error, format, args);
        }

        #endregion
    }
}
