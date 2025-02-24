using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace URMT.Core.UI {
    public static class GUIUtils {
        public static void CreateMultSelectDropdown(string label, string dropLabel, IList<string> options, IList<bool> selected) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, new GUIStyle() {
                margin = new RectOffset(5, 0, 5, 0)
            });
            GUILayout.BeginHorizontal(GUI.skin.textField, GUILayout.Height(19.5f));

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton) {
                fixedWidth = 25,
            };

            // Display selected items inline
            GUILayout.BeginHorizontal();
            for(int i = 0; i < options.Count; i++) {
                if(selected[i]) {
                    GUIStyle tagStyle = new GUIStyle(EditorStyles.miniButton) {
                        normal = { textColor = Color.black },
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(5, 5, 2, 2)
                    };

                    GUILayout.Label($" {options[i]} ", tagStyle);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Dropdown button on the right when items are selected
            if(GUILayout.Button("▼", buttonStyle)) {
                Rect mouseRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0);
                PopupWindow.Show(mouseRect, new MultSelectPopup(dropLabel, options, selected, EditorWindow.focusedWindow));
            }

            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        public static void DrawAssemblyDefinitionProperty(SerializedProperty property, Rect rect, float width) {
            string currentAssemblyName = property.stringValue;

            // Find the current assembly definition
            AssemblyDefinition currentAssemblyDefinition = AssemblyDefinition.All.FirstOrDefault(x => x.Name == currentAssemblyName);
            AssemblyDefinitionAsset currentAssemblyAsset = currentAssemblyDefinition != null ? AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(currentAssemblyDefinition.AssemblyPath) : null;

            // Create ObjectField for selecting AssemblyDefinitionAsset
            AssemblyDefinitionAsset newAssemblyAsset = (AssemblyDefinitionAsset)EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight),
                currentAssemblyAsset,
                typeof(AssemblyDefinitionAsset),
                false
            );

            // Update AssemblyName if a new assembly is selected
            if(newAssemblyAsset != null && newAssemblyAsset != currentAssemblyAsset) {
                property.stringValue = AssemblyDefinition.LoadFromAssemblyDefinitionAsset(newAssemblyAsset).Name;
            }
        }

        public static void DrawTitle(string text) {
            GUILayout.Space(10);

            GUIStyle headerLabelStyle = new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            GUILayout.Label(text, headerLabelStyle);
            GUILayout.Space(10);
        }
    }
}
