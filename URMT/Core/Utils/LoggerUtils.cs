using System;

namespace URMT.Core.Utils {
    public static class LoggerUtils {
        public static void Log(string message) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            Console.WriteLine(message);
        }
        public static void Log(string message, params object[] args) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            Console.WriteLine(string.Format(message, args));
        }

        public static void LogWarning(string message) {
            if (!CoreModule.Instance.EnableDebugLogging) return;

            Console.WriteLine(message);
        }
        public static void LogWarning(string message, params object[] args) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            Console.WriteLine(string.Format(message, args));
        }

        public static void LogError(string message) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            Console.Error.WriteLine(message);
        }
        public static void LogError(string message, params object[] args) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            Console.Error.WriteLine(string.Format(message, args));
        }
    }
}
