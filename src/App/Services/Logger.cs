using System;
using System.IO;

namespace AzureKvSslExpirationChecker.Services
{
    /// <summary>
    /// Simple file logger for the application.
    /// </summary>
    internal static class Logger
    {
        private static readonly object _lock = new();
        private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AzureKvSslExpirationChecker.log");

        /// <summary>Full path to the log file.</summary>
        public static string LogPath => _logPath;

        /// <summary>Append a message to the log file with a timestamp.</summary>
        public static void Log(string message)
        {
            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";
                lock (_lock)
                {
                    File.AppendAllText(_logPath, line);
                }
            }
            catch
            {
                // Never throw from the logger
            }
        }

        /// <summary>Append a message and exception details to the log file.</summary>
        public static void Log(string message, Exception ex) =>
            Log($"{message}: {ex}");
    }
}
