using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace URMT.Utility.Menus {
    public static class CopyMenu {
        [MenuItem("Assets/Copy #z", false, 0)]
        private static void Copy() {
            foreach(var obj in Selection.objects) {
                if(AssetDatabase.GetAssetPath(obj) == "Assets") {
                    Debug.LogWarning("Cannot copy the 'Assets' folder.");
                    return;
                }
            }

            CopyHandler.ObjectsToCopy = Selection.objects;
        }

        [MenuItem("Assets/Copy #z", true)]
        private static bool ValidateCopy() {
            return IsInProjectWindow();
        }

        [MenuItem("Assets/Paste #x", false, 1)]
        private static void Paste() {
            string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if(string.IsNullOrEmpty(selectionPath) || !AssetDatabase.IsValidFolder(selectionPath)) {
                selectionPath = GetActiveFolderPath();
            }

            CopyHandler.Paste(selectionPath);
        }

        [MenuItem("Assets/Paste #x", true)]
        private static bool ValidatePaste() {
            return IsInProjectWindow();
        }

        private static bool IsInProjectWindow() {
            return EditorWindow.focusedWindow != null &&
                   EditorWindow.focusedWindow.GetType().Name == "ProjectBrowser";
        }

        // Sense there is no public API to get the active folder in the Project window,
        // we have to use reflection to get it.
        private static string GetActiveFolderPath() {
            var projectBrowserType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser");
            if(projectBrowserType != null) {
                EditorWindow projectBrowser = EditorWindow.GetWindow(projectBrowserType);
                MethodInfo method = projectBrowserType.GetMethod("GetActiveFolderPath", BindingFlags.Instance | BindingFlags.NonPublic);
                if(method != null) {
                    string folderPath = method.Invoke(projectBrowser, null) as string;
                    if(!string.IsNullOrEmpty(folderPath))
                        return folderPath;
                }
            }
            

            return "Assets";
        }
    }
}
