using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;
using UnityRoundsModdingTools.Editor.Utils;

namespace UnityRoundsModdingTools.Editor.Windows {
    public class ModsListWindow : EditorWindow {
        private static Dictionary<string, bool> selectedMods;

        [MenuItem("Unity Rounds Modding Tools/Mods List")]
        private static void ShowWindow() {
            GetWindow(typeof(ModsListWindow), false, "Mods List");
        }

        private void OnEnable() {
            if(selectedMods == null) {
                selectedMods = GetAllAssemblyDefinitionPath()
                    .Select(AssemblyDefinition.Load)
                    .Where(a => !a.Name.StartsWith("UnityRoundsModdingTools"))
                    .ToDictionary(assembly => assembly.AssemblyPath, assembly => false);
            }
        }

        private void OnGUI() {
            GUILayout.Space(10);

            GUIStyle headerLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            headerLabelStyle.alignment = TextAnchor.MiddleCenter;
            headerLabelStyle.fontSize = 18;

            GUILayout.Label("Mods List", headerLabelStyle);

            GUILayout.Space(10);

            GUIUtils.DrawDictionaryEntries(0, ref selectedMods, (key, value) => {
                int index = selectedMods.Keys.ToList().IndexOf(key);

                GUILayout.Label($"{Path.GetFileNameWithoutExtension(Path.Combine(Directory.GetCurrentDirectory(), key))}:", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("Focus", GUILayout.MaxWidth(50), GUILayout.MaxHeight(15))) {
                    AssemblyDefinitionAsset loadedAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(key);
                    EditorGUIUtility.PingObject(loadedAsset);
                    Selection.activeObject = loadedAsset;
                }

                selectedMods[key] = GUILayout.Toggle(selectedMods[key], "");
            });

            var selectedAssemblyDefinitions = selectedMods
                .Where(kvp => kvp.Value)
                .Select(kvp => AssemblyDefinition.Load(kvp.Key));

            if(selectedAssemblyDefinitions.Count() > 0) {
                if(GUILayout.Button((selectedAssemblyDefinitions.Count() > 1) ? "Bulk Delete" : "Delete")) {
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine("Are you sure you want to delete the following mods?");
                    messageBuilder.AppendLine();

                    foreach(string assembly in selectedAssemblyDefinitions.Select(assembly => assembly.Name)) {
                        messageBuilder.AppendLine($"- {assembly}");
                    }

                    bool result = EditorUtility.DisplayDialog("Confirm Deletion", messageBuilder.ToString(), "Yes", "Cancel");
                    if(result) {
                        foreach(var assembly in selectedAssemblyDefinitions) {
                            ProjectMappings.Instance.folderMappings.RemoveAll(m => m.AssemblyName == assembly.Name);
                            Directory.GetParent(assembly.AssemblyPath).Delete(true);
                        }

                        AssetDatabase.Refresh();
                    }
                }
            }
        }

        public static string[] GetAllAssemblyDefinitionPath() {
            return AssetDatabase.GetAllAssetPaths()
                .Where(path => path.Contains(".asmdef") && !path.StartsWith("Packages"))
                .ToArray();
        }
    }
}
