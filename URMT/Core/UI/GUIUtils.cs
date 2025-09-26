using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace URMT.Core.UI {
    public static class GUIUtils {
        public static void CreateMultSelectDropdown(
            string label,
            string dropLabel,
            IList<string> options,
            IList<bool> selected
        ) {

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(label);

            EditorGUILayout.BeginHorizontal(GUI.skin.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

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
            if(GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(25))) {
                Rect mouseRect = new Rect(
                    Event.current.mousePosition.x,
                    Event.current.mousePosition.y,
                    0, 0);

                PopupWindow.Show(
                    mouseRect,
                    new MultSelectPopup(dropLabel, options, selected, EditorWindow.focusedWindow));
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
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


        public static void DrawAssetBundleProperty(SerializedProperty property, Rect rect) {
            string assetName = property.stringValue;

            string[] options = AssetDatabase.GetAllAssetBundleNames();
            if(!options.Contains(assetName)) {
                options = options.Append(String.IsNullOrWhiteSpace(property.stringValue) ? "None" : assetName).ToArray();
            }

            int selectedIndex = options.ToList().FindIndex(o => o == (String.IsNullOrWhiteSpace(property.stringValue) ? "None" : assetName));
            int selected = EditorGUI.Popup(rect, "Assets Bundle", selectedIndex, options);

            if(options[selectedIndex] != options[selected]) {
                property.stringValue = options[selected];
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

        public static void RenderIndented(Action renderAction) {
            float indentWidth = EditorGUI.indentLevel * 15f;

            GUILayout.BeginHorizontal();
            GUILayout.Space(indentWidth);
            GUILayout.BeginVertical();
            renderAction();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
