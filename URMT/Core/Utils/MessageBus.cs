using System;
using System.Collections.Generic;

namespace URMT.Core.Managers {
    /// <summary>
    /// Provides a centralized messaging system for registering and invoking actions 
    /// based on message identifiers, enabling decoupled method invocation.
    /// </summary>
    public static class MessageBus {
        private static readonly Dictionary<string, Action<object[]>> messageHandlers = new Dictionary<string, Action<object[]>>();

        public static void RegisterMessage(string message, Action<object[]> action) {
            if(messageHandlers.ContainsKey(message)) {
                messageHandlers[message] += action;
            } else {
                messageHandlers[message] = action;
            }
        }

        public static void SendMessage(string message, params object[] args) {
            if(messageHandlers.ContainsKey(message)) {
                messageHandlers[message]?.Invoke(args);
            }
        }
    }
}
