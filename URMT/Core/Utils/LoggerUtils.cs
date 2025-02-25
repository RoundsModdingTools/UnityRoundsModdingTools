using System;

namespace URMT.Core.Utils {
    public static class LoggerUtils {
        public static void Log(string message) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            UnityEngine.Debug.Log(message);
        }
        public static void Log(string message, params object[] args) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            UnityEngine.Debug.Log(string.Format(message, args));
        }

        public static void LogWarning(string message) {
            if (!CoreModule.Instance.EnableDebugLogging) return;

            UnityEngine.Debug.LogWarning(message);
        }
        public static void LogWarning(string message, params object[] args) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            UnityEngine.Debug.LogWarning(string.Format(message, args));
        }

        public static void LogError(string message) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            UnityEngine.Debug.LogError(message);
        }
        public static void LogError(string message, params object[] args) {
            if(!CoreModule.Instance.EnableDebugLogging) return;

            UnityEngine.Debug.LogError(string.Format(message, args));
        }
    }
}
