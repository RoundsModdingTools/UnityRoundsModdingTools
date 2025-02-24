using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using URMT.Core.Settings;

namespace URMT.Core.Managers {
    public static class SettingsManager {
        private static readonly Dictionary<ISettingMenu, bool> SettingMenus = new Dictionary<ISettingMenu, bool>();

        public static void RegisterSettingMenu(ISettingMenu settingMenu) {
            if(settingMenu == null)
                throw new ArgumentNullException(nameof(settingMenu));
            if(!(settingMenu is ScriptableObject))
                throw new ArgumentException("Setting menu must be a ScriptableObject", nameof(settingMenu));
            if(SettingMenus.ContainsKey(settingMenu))
                throw new ArgumentException("Setting menu already registered", nameof(settingMenu));

            SettingMenus.Add(settingMenu, true);
        }

        public static void RenderSettings() {
            var keys = new List<ISettingMenu>(SettingMenus.Keys);

            foreach(var key in keys) {
                if(!SettingMenus.ContainsKey(key)) continue;

                var scriptableSetting = (ScriptableObject)key;
                SettingMenus[key] = EditorGUILayout.Foldout(SettingMenus[key], key.Name, true, EditorStyles.foldout);

                if(!SettingMenus[key]) continue;

                EditorGUI.indentLevel++;
                SerializedObject serializedObject = new SerializedObject(scriptableSetting);
                serializedObject.Update();

                bool hasChanges = false;
                Type settingType = key.GetType();
                Dictionary<string, MethodInfo> renderMethods = GetRenderMethods(settingType);

                EditorGUI.BeginChangeCheck();
                SerializedProperty property = serializedObject.GetIterator();
                property.NextVisible(true);

                while(property.NextVisible(false)) {
                    if(renderMethods.TryGetValue(property.name, out MethodInfo method)) {
                        method.Invoke(key, new object[] { property });
                    } else {
                        EditorGUILayout.PropertyField(property, true);
                    }
                }

                if(EditorGUI.EndChangeCheck()) {
                    hasChanges = true;
                }

                if(hasChanges) {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(scriptableSetting);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        private static Dictionary<string, MethodInfo> GetRenderMethods(Type type) {
            Dictionary<string, MethodInfo> methods = new Dictionary<string, MethodInfo>();

            foreach(MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                var attribute = method.GetCustomAttribute<RenderForAttribute>();
                if(attribute != null && method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(SerializedProperty)) {
                    methods[attribute.FieldName] = method;
                }
            }
            return methods;
        }
    }
}
