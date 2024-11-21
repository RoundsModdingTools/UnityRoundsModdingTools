using BepInEx;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.CustomInspector {
    [CustomEditor(typeof(ModInfo))]
    internal class ModInfoInspector : UnityEditor.Editor {
        private ReorderableList dependenciesList;

        private void OnEnable() {
            SerializedProperty dependenciesProperty = serializedObject.FindProperty("dependencies");
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

            if(GUILayout.Button("Save")) {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }

            string modDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(modInfo));
            string readmePath = Path.Combine(modDirectory, "README.md");
            string iconPath = Path.Combine(modDirectory, "icon.png");

            if(!File.Exists(readmePath)) {
                EditorGUILayout.HelpBox("README file not found. Please create one.", MessageType.Error);
            }
            if(!File.Exists(iconPath)) {
                EditorGUILayout.HelpBox("Icon file not found. Please create one.", MessageType.Error);
            }

            if(GUILayout.Button("Publish") && !modName.stringValue.IsNullOrWhiteSpace()) {
                PublishMod(readmePath, iconPath);
                EditorUtility.RevealInFinder(Path.Combine(Settings.Instance.PublishPath, modName.stringValue));
            }
        }

        private void PublishMod(string readmePath, string iconPath) {
            ModInfo modInfo = (ModInfo)target;

            string publishPath = Path.Combine(Settings.Instance.PublishPath, modInfo.ModName);

            if(Directory.Exists(publishPath)) Directory.Delete(publishPath, true);
            if(File.Exists($"{publishPath}.zip")) File.Delete($"{publishPath}.zip");

            Directory.CreateDirectory(publishPath);

            string DllObjPath = GetDLLObjPath(modInfo.ModAssemblyDefinition.Assembly.Location);

            Manifest manifest = new Manifest {
                ModName = modInfo.ModName,
                Version = modInfo.Version,
                WebsiteURL = modInfo.WebsiteURL,
                Description = modInfo.Description,
                Dependencies = modInfo.dependencies,
            };

            File.Copy(DllObjPath, Path.Combine(publishPath, Path.GetFileName(DllObjPath)));
            File.WriteAllText(Path.Combine(publishPath, "manifest.json"), JsonConvert.SerializeObject(manifest, Formatting.Indented));
            File.Copy(readmePath, Path.Combine(publishPath, "README.md"));
            File.Copy(iconPath, Path.Combine(publishPath, "icon.png"));

            ZipFile.CreateFromDirectory(publishPath, $"{publishPath}.zip");

            if (!Settings.Instance.PublishFolderCopyTo.IsNullOrWhiteSpace() && Directory.Exists(Settings.Instance.PublishFolderCopyTo)) {
                string copyToPath = Path.Combine(Settings.Instance.PublishFolderCopyTo, $"Unknown-{modInfo.ModName}");
                if(Directory.Exists(copyToPath)) Directory.Delete(copyToPath, true);

                // Copy the folder to the specified path
                FileSystemManager.CopyDirectory(publishPath, copyToPath, new string[] { }, new string[] { });
            }
        }

        private string GetDLLObjPath(string dllPath) {
            string debugDllPath = $"obj/Debug/{Path.GetFileName(dllPath)}";
            string releaseDllPath = $"obj/Release/{Path.GetFileName(dllPath)}";

            bool debugExists = File.Exists(debugDllPath);
            bool releaseExists = File.Exists(releaseDllPath);

            if(debugExists && releaseExists) {
                DateTime debugLastModified = File.GetLastWriteTime(debugDllPath);
                DateTime releaseLastModified = File.GetLastWriteTime(releaseDllPath);

                if(debugLastModified > releaseLastModified) {
                    return debugDllPath;
                } else {
                    return releaseDllPath;
                }
            }

            if(debugExists) return debugDllPath;
            if(releaseExists) return releaseDllPath;

            return null;
        }
    }

    public struct Manifest {

        [JsonProperty("name")] public string ModName;
        [JsonProperty("version_number")] public string Version;
        [JsonProperty("website_url")] public string WebsiteURL;
        [JsonProperty("description")] public string Description;
        [JsonProperty("dependencies")] public string[] Dependencies;
    }
}
