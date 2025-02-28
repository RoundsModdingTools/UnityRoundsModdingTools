using UnityEditor;
using System.IO;

namespace URMT.Utility.Menus {
    public static class CopySystem {
        [MenuItem("Assets/Copy #z", false, 0)]
        public static void Copy() {
            CopyHandler.ObjectsToCopy = Selection.objects;
        }

        [MenuItem("Assets/Copy #z", true)]
        private static bool ValidateCopy() {
            return IsInProjectWindow();
        }

        [MenuItem("Assets/Paste #x", false, 1)]
        public static void Paste() {
            var selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            // Ensure we're dealing with a folder
            if(!AssetDatabase.IsValidFolder(selectionPath)) {
                selectionPath = Path.GetDirectoryName(selectionPath);
            }

            CopyHandler.Paste(selectionPath);
        }

        [MenuItem("Assets/Paste #x", true)]
        private static bool ValidatePaste() {
            return IsInProjectWindow() && Selection.activeObject != null;
        }

        private static bool IsInProjectWindow() {
            return EditorWindow.focusedWindow != null &&
                   EditorWindow.focusedWindow.GetType().Name == "ProjectBrowser";
        }
    }
}
