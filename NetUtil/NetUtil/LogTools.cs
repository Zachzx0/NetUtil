using System;

namespace NetUtil.Log
{
    public static class LogTools
    {
        public delegate void LogEventHandler(string log);
        public static event LogEventHandler LogHandler;
        public static event LogEventHandler LogWarningHandler;
        public static event LogEventHandler LogErrorHandler;

        internal static void Log(string log)
        {
            Console.WriteLine(log);

            if (LogHandler != null)
            {
                LogHandler.Invoke(log);
            }
        }

        internal static void LogWaring(string log)
        {
            Console.WriteLine(log);

            if (LogWarningHandler != null)
            {
                LogWarningHandler.Invoke(log);
            }
        }
        internal static void LogError(string log)
        {
            Console.WriteLine(log);

            if (LogErrorHandler != null)
            {
                LogErrorHandler.Invoke(log);
            }
        }

        internal static void LogFormat(string log,params object[] args)
        {
            string newLog = string.Format(log, args);
            Log(newLog);
        }

        internal static void LogErrorFormat(string log, params object[] args)
        {
            string newLog = string.Format(log, args);
            LogError(newLog);
        }
    }
}
