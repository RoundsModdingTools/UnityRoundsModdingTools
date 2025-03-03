using System.Collections.Concurrent;
using System;
using System.Threading.Tasks;
using UnityEditor;

namespace URMT.Core.Utils {
    [InitializeOnLoad]
    public static class MainThreadAction {
        private static readonly ConcurrentQueue<Action> MainThreadActions = new ConcurrentQueue<Action>();

        public static void Invoke(Action action) {
            MainThreadActions.Enqueue(action);
        }

        public static async void InvokeAsync(Action action) {
            var tcs = new TaskCompletionSource<bool>();
            MainThreadActions.Enqueue(() => {
                try {
                    action?.Invoke();
                    tcs.SetResult(true);
                } catch(Exception e) {
                    tcs.SetException(e);
                }
            });
            await tcs.Task;
        }

        public static async Task<T> InvokeAsync<T>(Func<T> func) {
            var tcs = new TaskCompletionSource<T>();
            MainThreadActions.Enqueue(() => {
                try {
                    T result = func();
                    tcs.SetResult(result);
                } catch(Exception e) {
                    tcs.SetException(e);
                }
            });
            return await tcs.Task;
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
