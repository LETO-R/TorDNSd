using System;

namespace TorDNSd.Logging
{
    /// <summary>
    /// Logger class.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Event raised whenever a message is logged.
        /// </summary>
        public static event Action<LogSeverity,  string> OnLog;

        /// <summary>
        /// Minimum log severity to report.
        /// </summary>
        public static LogSeverity MinLogSeverity = LogSeverity.Info;

        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="severity">Severity.</param>
        /// <param name="message">Message.</param>
        /// <param name="args">Format arguments.</param>
        public static void Log(LogSeverity severity, string message, params string[] args)
        {
            if (OnLog != null)
            {
                OnLog(severity, string.Format(message, args));
            }
        }
    }
}
