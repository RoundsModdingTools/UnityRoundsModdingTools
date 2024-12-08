using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.CustomInspector {
    [CustomEditor(typeof(ModInfo))]
    internal class ModInfoInspector : UnityEditor.Editor {
        private ReorderableList dependenciesList;
        private ReorderableList dllDependenciesList;

        public void OnEnable() {
            SerializedProperty dependenciesProperty = serializedObject.FindProperty(nameof(ModInfo.Dependencies));
            SetupDependenciesList(dependenciesProperty);
            SerializedProperty dllDependenciesProperty = serializedObject.FindProperty(nameof(ModInfo.DllDependencies));
            SetupDllDependenciesList(dllDependenciesProperty);
        }

        void SetupDllDependenciesList(SerializedProperty dllDependenciesProperty) {
            ModInfo modInfo = (ModInfo)target;

            dllDependenciesList = new ReorderableList(serializedObject, dllDependenciesProperty, true, true, true, true);

            dllDependenciesList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "DLL Dependencies");
            };
            dllDependenciesList.drawElementCallback = (rect, index, isActive, isFocused) => {
                SerializedProperty element = dllDependenciesProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                rect.height = EditorGUIUtility.singleLineHeight;

                List<string> availablePrecompiledAssemblies = CompilationPipeline.GetPrecompiledAssemblyNames()
                    .OrderBy(x => x)
                    .ToList();

                List<GUIContent> availablePrecompiledAssembliesContent = availablePrecompiledAssemblies.Select(x => new GUIContent(x)).ToList();


                int selectedIndex = availablePrecompiledAssemblies.IndexOf(modInfo.DllDependencies[index]);
                if(selectedIndex == -1) {
                    availablePrecompiledAssembliesContent.Insert(0, new GUIContent("None"));
                    selectedIndex = 0;
                }

                modInfo.DllDependencies[index] = availablePrecompiledAssembliesContent[EditorGUI.Popup(rect, selectedIndex, availablePrecompiledAssembliesContent.ToArray())].text;
            };
            dllDependenciesList.onAddCallback = list => {
                int index = dllDependenciesProperty.arraySize;
                dllDependenciesProperty.InsertArrayElementAtIndex(index);
                SerializedProperty newElement = dllDependenciesProperty.GetArrayElementAtIndex(index);
                newElement.stringValue = string.Empty;
                serializedObject.ApplyModifiedProperties();
            };
            dllDependenciesList.onRemoveCallback = list => {
                if(dllDependenciesProperty.arraySize > 0) {
                    dllDependenciesProperty.DeleteArrayElementAtIndex(list.index);
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }

        void SetupDependenciesList(SerializedProperty dependenciesProperty) {
            dependenciesList = new ReorderableList(serializedObject, dependenciesProperty, true, true, true, true);

            dependenciesList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Dependencies");
            };

            dependenciesList.drawElementCallback = (rect, index, isActive, isFocused) => {
                SerializedProperty element = dependenciesProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(rect, element, GUIContent.none);
            };

            dependenciesList.onAddCallback = list => {
                int index = dependenciesProperty.arraySize;
                dependenciesProperty.InsertArrayElementAtIndex(index);
                SerializedProperty newElement = dependenciesProperty.GetArrayElementAtIndex(index);
                newElement.stringValue = string.Empty;
                serializedObject.ApplyModifiedProperties();
            };

            dependenciesList.onRemoveCallback = list => {
                if(dependenciesProperty.arraySize > 0) {
                    dependenciesProperty.DeleteArrayElementAtIndex(list.index);
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }

        public override void OnInspectorGUI() {
            ModInfo modInfo = (ModInfo)target;
            if(modInfo.ModAssemblyDefinition == null) {
                EditorGUILayout.HelpBox("Assembly Definition not found. Please create one.", MessageType.Error);
                return;
            }

            SerializedProperty modName = serializedObject.FindProperty("ModName");
            SerializedProperty version = serializedObject.FindProperty("Version");
            SerializedProperty websiteURL = serializedObject.FindProperty("WebsiteURL");
            SerializedProperty description = serializedObject.FindProperty("Description");

            string[] MajorMinorPatch = version.stringValue.Split('.');

            modName.stringValue = EditorGUILayout.TextField("Mod Name", modName.stringValue);

            // Version Field (Editable major, minor, patch parts)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Version", GUILayout.Width(60));
            GUILayout.FlexibleSpace();
            MajorMinorPatch[0] = EditorGUILayout.TextField(MajorMinorPatch[0], GUILayout.Width(50));
            EditorGUILayout.LabelField(".", GUILayout.Width(10));
            MajorMinorPatch[1] = EditorGUILayout.TextField(MajorMinorPatch[1], GUILayout.Width(50));
            EditorGUILayout.LabelField(".", GUILayout.Width(10));
            MajorMinorPatch[2] = EditorGUILayout.TextField(MajorMinorPatch[2], GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            version.stringValue = string.Join(".", MajorMinorPatch);

            websiteURL.stringValue = EditorGUILayout.TextField("Website URL", websiteURL.stringValue);

            description.stringValue = EditorGUILayout.TextField("Description", description.stringValue);

            GUILayout.Space(10);
            dependenciesList.DoLayoutList();
            dllDependenciesList.DoLayoutList();

            if(GUILayout.Button("Save")) {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }

            string modDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(modInfo));
            string readmePath = Path.Combine(modDirectory, "README.md");
            string iconPath = Path.Combine(modDirectory, "icon.png");

            if(!File.Exists(readmePath)) {
                EditorGUILayout.HelpBox("README file not found. Recommend creating one.", MessageType.Warning);
            }
            if(!File.Exists(iconPath)) {
                EditorGUILayout.HelpBox("Icon file not found. Recommend creating one.", MessageType.Warning);
            }

            if(GUILayout.Button("Publish") && !modName.stringValue.IsNullOrWhiteSpace()) {
                modInfo.PublishMod();
                EditorUtility.RevealInFinder(Path.Combine(Settings.Instance.PublishPath, modName.stringValue));
            }
        }
    }

    public struct Manifest {

        [JsonProperty("name")] public string ModName;
        [JsonProperty("version_number")] public string Version;
        [JsonProperty("website_url")] public string WebsiteURL;
        [JsonProperty("description")] public string Description;
        [JsonProperty("dependencies")] public List<string> Dependencies;
    }
}
