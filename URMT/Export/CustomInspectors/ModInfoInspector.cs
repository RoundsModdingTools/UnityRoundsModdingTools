using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using URMT.Core.UI;
using URMT.Export.ScriptableObjects;

namespace URMT.Export.CustomInspectors {
    [CustomEditor(typeof(ModInfo))]
    internal class ModInfoInspector : Editor {
        private ReorderableList dependenciesList;

        private ReorderableList dllDependenciesList;
        private ReorderableList assemblyDefinitionDependenciesList;

        private bool manifestFoldout = true;
        private bool dependenciesFoldout = true;
        private bool exportSettingsFoldout = true;

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

        private void RenderManifestFields() {
            ModInfo modInfo = (ModInfo)target;

            SerializedProperty modName = serializedObject.FindProperty(nameof(ModInfo.ModName));
            SerializedProperty version = serializedObject.FindProperty(nameof(ModInfo.Version));
            SerializedProperty websiteURL = serializedObject.FindProperty(nameof(ModInfo.WebsiteURL));
            SerializedProperty description = serializedObject.FindProperty(nameof(ModInfo.Description));
            SerializedProperty includeInAllExports = serializedObject.FindProperty(nameof(ModInfo.IncludeInAllExports));
            SerializedProperty beforeBuildCommand = serializedObject.FindProperty(nameof(ModInfo.BeforeBuildCommand));
            SerializedProperty afterBuildCommand = serializedObject.FindProperty(nameof(ModInfo.AfterBuildCommand));
            SerializedProperty iconTextureProp = serializedObject.FindProperty(nameof(ModInfo.Icon));

            EditorGUILayout.PropertyField(iconTextureProp);
            if(iconTextureProp != null) {
                string path = AssetDatabase.GetAssetPath(iconTextureProp.objectReferenceValue);
                if(!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) {
                    EditorGUILayout.HelpBox("Assigned texture must be a PNG file (.png).", MessageType.Error);
                }
            }
            modName.stringValue = EditorGUILayout.TextField("Mod Name", modName.stringValue);

            // Version Field (Editable major, minor, patch parts)
            string[] MajorMinorPatch = version.stringValue.Split('.');
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

            dependenciesList.DoLayoutList();
        }

        private void RenderDependenciesFields() {
            dllDependenciesList.DoLayoutList();
            assemblyDefinitionDependenciesList.DoLayoutList();
        }

        private void RenderExportSettings() {
            ModInfo modInfo = (ModInfo)target;

            SerializedProperty modName = serializedObject.FindProperty(nameof(ModInfo.ModName));
            SerializedProperty includeInAllExports = serializedObject.FindProperty(nameof(ModInfo.IncludeInAllExports));
            SerializedProperty beforeBuildCommand = serializedObject.FindProperty(nameof(ModInfo.BeforeBuildCommand));
            SerializedProperty afterBuildCommand = serializedObject.FindProperty(nameof(ModInfo.AfterBuildCommand));
            SerializedProperty author = serializedObject.FindProperty(nameof(ModInfo.Author));

            author.stringValue = EditorGUILayout.TextField("Author", author.stringValue);
            includeInAllExports.boolValue = EditorGUILayout.Toggle("Include In All Exports", includeInAllExports.boolValue);
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Pre Build Command");
            beforeBuildCommand.stringValue = EditorGUILayout.TextArea(beforeBuildCommand.stringValue, GUILayout.Height(100));
            GUILayout.Label("Post Build Command");
            afterBuildCommand.stringValue = EditorGUILayout.TextArea(afterBuildCommand.stringValue, GUILayout.Height(100));

            if(EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                modInfo.CreatePowerShellScript();
                modInfo.GenerateScripts();
            }

            string modDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(modInfo));
            string readmePath = Path.Combine(modDirectory, "README.md");
            string iconPath = Path.Combine(modDirectory, "icon.png");

            if (!Application.platform.ToString().Contains("Windows")) {
                EditorGUILayout.HelpBox("PowerShell scripts are only supported on Windows. These scripts will not run on other platforms.", MessageType.Warning);
            }
            if(!modInfo.HasReadme) {
                EditorGUILayout.HelpBox("README file not found. Recommend creating one.", MessageType.Warning);
            }
            if(!modInfo.HasIcon) {
                EditorGUILayout.HelpBox("Icon file not found. Recommend creating one.", MessageType.Warning);
            }

            if(GUILayout.Button("Export") && !string.IsNullOrWhiteSpace(modName.stringValue)) {
                modInfo.ExportMod();
                EditorUtility.RevealInFinder(Path.Combine(ExportModuleSettings.Instance.ExportPath, modName.stringValue));
            }

            if(modInfo.ModAssemblyDefinition == null) {
                EditorGUILayout.HelpBox("The mod's Assembly Definition was not found in the root directory. As a result, the \"Assembly Definition Dependencies\" will apply to the \"plugins\" folder instead of the \"dependencies\" folder.", MessageType.Info);
            }
        }

        public override void OnInspectorGUI() {
            ModInfo modInfo = (ModInfo)target;

            manifestFoldout = EditorGUILayout.Foldout(manifestFoldout, "Manifest Fields");
            if(manifestFoldout) {
                RenderManifestFields();
            }

            dependenciesFoldout = EditorGUILayout.Foldout(dependenciesFoldout, "Dependencies Fields");
            if(dependenciesFoldout) {
                RenderDependenciesFields();
            }

            exportSettingsFoldout = EditorGUILayout.Foldout(exportSettingsFoldout, "Export Settings");
            if(exportSettingsFoldout) {
                RenderExportSettings();
            }

            if(GUI.changed) {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
