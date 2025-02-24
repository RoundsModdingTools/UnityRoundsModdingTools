using UnityEditor;
using UnityEngine;
using URMT.Core.Managers;

namespace URMT.Core.UI.Windows {
    public class SettingsWindow : EditorWindow {
        private static Vector2 scrollPosition;

        [MenuItem("URMT/Settings")]
        public static void ShowWindow() {
            GetWindow<SettingsWindow>("URMT Settings");
        }
        private void OnGUI() {
            GUIUtils.DrawTitle("URMT Settings");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(EditorGUIUtility.currentViewWidth), GUILayout.Height(Screen.height - 30));

            SettingsManager.RenderSettings();

            EditorGUILayout.EndScrollView();
        }
    }
}
