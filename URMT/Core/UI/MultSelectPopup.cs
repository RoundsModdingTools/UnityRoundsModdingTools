using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace URMT.Core.UI {
    internal class MultSelectPopup : PopupWindowContent {
        private string label;

        private IList<string> options;
        private IList<bool> selected;

        private Vector2 scrollPosition;

        private EditorWindow caller;

        public MultSelectPopup(string label, IList<string> options, IList<bool> selected, EditorWindow caller) {
            this.label = label;

            this.options = options;
            this.selected = selected;

            this.caller = caller;
        }

        public override Vector2 GetWindowSize() {
            return new Vector2(200, Mathf.Min(options.Count * 20 + 40, 300));
        }

        public override void OnGUI(Rect rect) {
            GUILayout.Label(label, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Scrollable area for the list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(200), GUILayout.Height(250));
            for(int i = 0; i < options.Count; i++) {
                bool newSelected = EditorGUILayout.ToggleLeft(options[i], selected[i]);
                if(newSelected != selected[i]) {
                    selected[i] = newSelected;

                    caller.Repaint();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

}
