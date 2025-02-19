using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace UnityRoundsModdingTools.Editor.Utils {
    [InitializeOnLoad]
    public static class MainThreadAction {
        private static readonly ConcurrentQueue<Action> MainThreadActions = new ConcurrentQueue<Action>();

        public static void Invoke(Action action) {
            MainThreadActions.Enqueue(action);
        }

        static MainThreadAction() {
            EditorApplication.update += ProcessMainThreadActions;
        }

        private static void ProcessMainThreadActions() {
            while(MainThreadActions.TryDequeue(out var action)) {
                action?.Invoke();
            }
        }
    }
}
