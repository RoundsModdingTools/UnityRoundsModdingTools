using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace UnityRoundsModdingTools.Editor.Utils {
    // Multi-Select Popup Class
    public class MultSelectPopup : PopupWindowContent {
        private string label;
        private List<string> options;
        private List<bool> selected;
        private Vector2 scrollPosition;

        public MultSelectPopup(string label, List<string> options, List<bool> selected) {
            this.label = label;
            this.options = options;
            this.selected = selected;
        }

        public override Vector2 GetWindowSize() {
            return new Vector2(200, Mathf.Min(options.Count * 20 + 40, 300)); // Limit max height
        }

        public override void OnGUI(Rect rect) {
            GUILayout.Label(label, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Scrollable area for the list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(200), GUILayout.Height(250));
            for(int i = 0; i < options.Count; i++) {
                selected[i] = EditorGUILayout.ToggleLeft(options[i], selected[i]);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    // GUI Utilities Class
    public static class GUIUtils {
        private static Dictionary<int, Vector2> _scrollPositions = new Dictionary<int, Vector2>();

        public static void DrawTitle(string text) {
            GUILayout.Space(10);

            GUIStyle headerLabelStyle = new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            GUILayout.Label(text, headerLabelStyle);
            GUILayout.Space(10);
        }

        public static void CreateTextInput(string label, ref string value) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(120));
            value = EditorGUILayout.TextField(value);
            GUILayout.EndHorizontal();
        }

        public static void DrawListEntries<T>(int id, ref List<T> list, Action<T> processEntry, int maxHeight = 200) {
            if(list.Count == 0) return;

            GUILayout.BeginVertical(GUI.skin.box);
            _scrollPositions[id] = EditorGUILayout.BeginScrollView(
                _scrollPositions.TryGetValue(id, out Vector2 value) ? value : Vector2.zero,
                GUILayout.MaxHeight(maxHeight)
            );

            foreach(var item in list) {
                GUILayout.BeginHorizontal();
                processEntry.Invoke(item);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        public static void DrawDictionaryEntries<TKey, TValue>(int id, ref Dictionary<TKey, TValue> dictionary, Action<TKey, TValue> processEntry, int maxHeight = 200) {
            if(dictionary.Count == 0) return;

            GUILayout.BeginVertical(GUI.skin.box);
            _scrollPositions[id] = EditorGUILayout.BeginScrollView(
                _scrollPositions.TryGetValue(id, out Vector2 value) ? value : Vector2.zero,
                GUILayout.MaxHeight(maxHeight)
            );

            List<TKey> keys = new List<TKey>(dictionary.Keys);

            foreach(var key in keys) {
                GUILayout.BeginHorizontal();
                processEntry.Invoke(key, dictionary[key]);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        // Multi-Select Dropdown with Dynamic Positioning
        public static void CreateMultSelectDropdown(string label, List<string> options, List<bool> selected) {
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
            GUILayout.FlexibleSpace(); // Ensures dropdown button stays on the right
            GUILayout.EndHorizontal();

            // Dropdown button on the right when items are selected
            if(GUILayout.Button("▼", buttonStyle)) {
                Rect mouseRect = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0);
                PopupWindow.Show(mouseRect, new MultSelectPopup(label, options, selected));
            }

            GUILayout.EndHorizontal();
        }

    }
}
