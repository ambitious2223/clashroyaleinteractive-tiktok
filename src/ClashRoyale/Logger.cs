using System;
using System.IO;
using NLog;
using SharpRaven.Data;

namespace ClashRoyale
{
    public class Logger
    {
#if DEBUG
        private static readonly object ConsoleSync = new object();
#endif

        private static NLog.Logger _logger;

        public Logger()
        {
            Directory.CreateDirectory("Logs");

            _logger = LogManager.GetCurrentClassLogger();
        }

        public static void Log(object message, Type type, ErrorLevel logType = ErrorLevel.Info)
        {
            switch (logType)
            {
                case ErrorLevel.Info:
                {
                    _logger.Info(message);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{logType}] {message}");
                    Console.ResetColor();
                    break;
                }

                case ErrorLevel.Warning:
                {
                    _logger.Warn(message);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[{logType}] {message}");
                    Console.ResetColor();
                    Resources.Sentry.Report(message.ToString(), type, logType);
                    break;
                }

                case ErrorLevel.Error:
                {
                    _logger.Error(message);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{logType}] {message}");
                    Console.ResetColor();
                    Resources.Sentry.Report(message.ToString(), type, logType);
                    break;
                }

                case ErrorLevel.Debug:
                {
                    _logger.Debug(message);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[{logType}] {message}");
                    Console.ResetColor();
                    break;
                }
            }
        }
    }
}