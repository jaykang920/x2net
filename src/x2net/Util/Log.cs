// Copyright (c) 2017 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2net
{
    /// <summary>
    /// Specifies the log level.
    /// </summary>
    public enum LogLevel
    {
        All,
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        None,
    }

    /// <summary>
    /// Represents the method that handles log calls.
    /// </summary>
    public delegate void LogHandler(LogLevel level, string message);

    /// <summary>
    /// Represents the logging helper class.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Gets or sets the application-provided log handler.
        /// </summary>
        public static LogHandler Handler { get; set; }

        public static void Emit(LogLevel level, string message)
        {
            if (Handler == null || Config.LogLevel > level)
            {
                return;
            }
            Handler(level, message);
        }

        public static void Emit(LogLevel level, string format, object arg0)
        {
            if ((Handler == null) || Config.LogLevel > level)
            {
                return;
            }
            Handler(level, String.Format(format, arg0));
        }

        public static void Emit(LogLevel level, string format, object arg0, object arg1)
        {
            if ((Handler == null) || Config.LogLevel > level)
            {
                return;
            }
            Handler(level, String.Format(format, arg0, arg1));
        }

        public static void Emit(LogLevel level, string format, object arg0, object arg1, object arg2)
        {
            if ((Handler == null) || Config.LogLevel > level)
            {
                return;
            }
            Handler(level, String.Format(format, arg0, arg1, arg2));
        }

        public static void Emit(LogLevel level, string format, params object[] args)
        {
            if ((Handler == null) || Config.LogLevel > level)
            {
                return;
            }
            Handler(level, String.Format(format, args));
        }

        #region Trace

        public static void Trace(string message)
        {
            Emit(LogLevel.Trace, message);
        }

        public static void Trace(string format, object arg0)
        {
            Emit(LogLevel.Trace, format, arg0);
        }

        public static void Trace(string format, object arg0, object arg1)
        {
            Emit(LogLevel.Trace, format, arg0, arg1);
        }

        public static void Trace(string format, object arg0, object arg1, object arg2)
        {
            Emit(LogLevel.Trace, format, arg0, arg1, arg2);
        }

        public static void Trace(string format, params object[] args)
        {
            Emit(LogLevel.Trace, format, args);
        }

        #endregion

        #region Debug

        public static void Debug(string message)
        {
            Emit(LogLevel.Debug, message);
        }

        public static void Debug(string format, object arg0)
        {
            Emit(LogLevel.Debug, format, arg0);
        }

        public static void Debug(string format, object arg0, object arg1)
        {
            Emit(LogLevel.Debug, format, arg0, arg1);
        }

        public static void Debug(string format, object arg0, object arg1, object arg2)
        {
            Emit(LogLevel.Debug, format, arg0, arg1, arg2);
        }

        public static void Debug(string format, params object[] args)
        {
            Emit(LogLevel.Debug, format, args);
        }

        #endregion

        #region Info

        public static void Info(string message)
        {
            Emit(LogLevel.Info, message);
        }

        public static void Info(string format, object arg0)
        {
            Emit(LogLevel.Info, format, arg0);
        }

        public static void Info(string format, object arg0, object arg1)
        {
            Emit(LogLevel.Info, format, arg0, arg1);
        }

        public static void Info(string format, object arg0, object arg1, object arg2)
        {
            Emit(LogLevel.Info, format, arg0, arg1, arg2);
        }

        public static void Info(string format, params object[] args)
        {
            Emit(LogLevel.Info, format, args);
        }

        #endregion

        #region Warn

        public static void Warn(string message)
        {
            Emit(LogLevel.Warning, message);
        }

        public static void Warn(string format, object arg0)
        {
            Emit(LogLevel.Warning, format, arg0);
        }

        public static void Warn(string format, object arg0, object arg1)
        {
            Emit(LogLevel.Warning, format, arg0, arg1);
        }

        public static void Warn(string format, object arg0, object arg1, object arg2)
        {
            Emit(LogLevel.Warning, format, arg0, arg1, arg2);
        }

        public static void Warn(string format, params object[] args)
        {
            Emit(LogLevel.Warning, format, args);
        }

        #endregion

        #region Error

        public static void Error(string message)
        {
            Emit(LogLevel.Error, message);
        }

        public static void Error(string message, Exception exception)
        {
            Emit(LogLevel.Error, String.Format("{0} {1}", message, exception.ToString()));
        }

        public static void Error(string format, object arg0)
        {
            Emit(LogLevel.Error, format, arg0);
        }

        public static void Error(string format, object arg0, object arg1)
        {
            Emit(LogLevel.Error, format, arg0, arg1);
        }

        public static void Error(string format, object arg0, object arg1, object arg2)
        {
            Emit(LogLevel.Error, format, arg0, arg1, arg2);
        }

        public static void Error(string format, params object[] args)
        {
            Emit(LogLevel.Error, format, args);
        }

        #endregion
    }
}
