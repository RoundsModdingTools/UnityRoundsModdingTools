using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityRoundsModdingTools.Editor.Utils {
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
    }
}
