using Sirenix.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityRoundsModdingTools.ScriptableObjects;
using UnityRoundsModdingTools.Utils;

namespace UnityRoundsModdingTools.Windows {
    public class ModsListWindow : EditorWindow {
        private Dictionary<string, bool> selectedMods = new Dictionary<string, bool>();

        [MenuItem("Unity Rounds Modding Tools/Mods List")]
        private static void ShowWindow() {
            GetWindow(typeof(ModsListWindow), false, "Mods List");
        }

        void OnGUI() {
            GUILayout.Space(10);

            GUIStyle headerLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            headerLabelStyle.alignment = TextAnchor.MiddleCenter;
            headerLabelStyle.fontSize = 18;

            GUILayout.Label("Mods List", headerLabelStyle);

            GUILayout.Space(10);

            string[] assemblyDefinitionPaths = AssemblyDefinitionUtils.GetAllAssemblyDefinitionPath();
            assemblyDefinitionPaths.ForEach(path => {
                if(!selectedMods.ContainsKey(path)) {
                    selectedMods.Add(path, false);
                }
            });


            GUIUtils.DrawDictionaryEntries(0, ref selectedMods, (key, value) => {
                int index = assemblyDefinitionPaths.ToList().IndexOf(key);

                GUILayout.Label($"{Path.GetFileNameWithoutExtension(Path.Combine(Directory.GetCurrentDirectory(), key))}:", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Focus", GUILayout.MaxWidth(50), GUILayout.MaxHeight(15))) {
                    AssemblyDefinitionAsset loadedAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(key);
                    EditorGUIUtility.PingObject(loadedAsset);
                    Selection.activeObject = loadedAsset;
                }

                selectedMods[key] = GUILayout.Toggle(selectedMods[key], "");
            });
            
            List<string> selectedKeys = selectedMods.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

            if(selectedKeys.Count > 0) {
                if(GUILayout.Button((selectedKeys.Count > 1) ? "Bulk Delete" : "Delete")) {
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine("Are you sure you want to delete the following mods?");
                    messageBuilder.AppendLine();

                    foreach(string modToDelete in selectedKeys) {
                        messageBuilder.AppendLine($"- {Path.GetFileNameWithoutExtension(modToDelete)}");
                    }


                    bool result = EditorUtility.DisplayDialog("Confirm Deletion", messageBuilder.ToString(), "Yes", "Cancel");
                    if(result) {
                        foreach(string modToDelete in selectedKeys) {
                            var assemblyDefinitionClass = AssemblyDefinitionUtils.ParseAssemblyDefinitionFie(modToDelete);
                            ProjectMappings.Instance.folderMappings.RemoveAll(m => m.AssemblyName == assemblyDefinitionClass.name);

                            Directory.GetParent(modToDelete).Delete(true);
                        }
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}
