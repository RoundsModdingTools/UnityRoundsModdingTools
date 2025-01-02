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
using UnityRoundsModdingTools.Editor.Utils;

namespace UnityRoundsModdingTools.Editor.CustomInspector {
    [CustomEditor(typeof(ModInfo))]
    internal class ModInfoInspector : UnityEditor.Editor {
        private ReorderableList dependenciesList;

        private ReorderableList dllDependenciesList;
        private ReorderableList assemblyDefinitionDependenciesList;

        public void OnEnable() {
            SerializedProperty dependenciesProperty = serializedObject.FindProperty(nameof(ModInfo.Dependencies));
            SetupDependenciesList(dependenciesProperty);
            SerializedProperty dllDependenciesProperty = serializedObject.FindProperty(nameof(ModInfo.DllDependencies));
            SetupDllDependenciesList(dllDependenciesProperty);
            SerializedProperty assemblyDefinitionDependenciesProperty = serializedObject.FindProperty(nameof(ModInfo.AssemblyDefinitionDependencies));
            SetupAssemblyDefinitionDependenciesList(assemblyDefinitionDependenciesProperty);
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

        void SetupAssemblyDefinitionDependenciesList(SerializedProperty assemblyDefinitionDependenciesProperty) {
            assemblyDefinitionDependenciesList = new ReorderableList(serializedObject, assemblyDefinitionDependenciesProperty, true, true, true, true);
            assemblyDefinitionDependenciesList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "AssemblyDefinition Dependencies");
            };
            assemblyDefinitionDependenciesList.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = assemblyDefinitionDependenciesProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                float halfWidth = rect.width / 2 - 10;

                // Get the current AssemblyName
                GUIUtils.DrawAssemblyDefinitionProperty(element, rect, rect.width - 10);
            };
            assemblyDefinitionDependenciesList.onAddCallback = list => {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;

                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.stringValue = "";
            };
            assemblyDefinitionDependenciesList.onRemoveCallback = list => {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            };
        }

        public override void OnInspectorGUI() {
            ModInfo modInfo = (ModInfo)target;

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
            assemblyDefinitionDependenciesList.DoLayoutList();

            if(GUI.changed) {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(modInfo);
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

            if(modInfo.ModAssemblyDefinition == null) {
                EditorGUILayout.HelpBox("The mod's Assembly Definition was not found in the root directory. As a result, the \"Assembly Definition Dependencies\" will apply to the \"plugins\" folder instead of the \"dependencies\" folder.", MessageType.Info);
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
