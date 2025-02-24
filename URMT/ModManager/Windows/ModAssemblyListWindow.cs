using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using URMT.Core;
using URMT.Core.Managers;
using URMT.Core.UI;

namespace URMT.ModManager.Windows {
    public class ModAssemblyListWindow : EditorWindow {
        private static Dictionary<string, bool> selectedMods;
        private static readonly string[] ingoredGetAllAssemblyDefinition = new string[] {
            "URMT" ,
            "ThunderstoreAPI",
            "GitHubAPI",
        };

        private static ReorderableList modAssemblyList;
        private static Vector2 scrollPosition;

        [MenuItem("URMT/Mod Manager/Mod Assembly List")]
        public static void ShowWindow() {
            GetWindow<ModAssemblyListWindow>("Mod Assembly List");
        }

        private void OnEnable() {
            if(selectedMods == null) {
                selectedMods = GetAllAssemblyDefinitionPath()
                    .Select(path => AssemblyDefinition.Load(path))
                    .Where(asmDef => asmDef != null && !ingoredGetAllAssemblyDefinition.Any(ignored => asmDef.Name.StartsWith(ignored)))
                    .ToDictionary(asmDef => asmDef.AssemblyPath, asmDef => false);

                CreateModList();
            }
        }

        private static string[] GetAllAssemblyDefinitionPath() {
            return AssetDatabase.GetAllAssetPaths()
                .Where(path => path.Contains(".asmdef") && !path.StartsWith("Packages"))
                .ToArray();
        }

        private void OnGUI() {
            GUIUtils.DrawTitle("Mod Assembly List");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(EditorGUIUtility.currentViewWidth), GUILayout.Height(Screen.height - 30));

            modAssemblyList.DoLayoutList();

            var selectedAssemblyDefinitions = selectedMods
                .Where(kvp => kvp.Value)
                .Select(kvp => AssemblyDefinition.Load(kvp.Key))
                .ToList();

            if(selectedAssemblyDefinitions.Count() > 0) {
                if(GUILayout.Button((selectedAssemblyDefinitions.Count() > 1) ? "Bulk Delete" : "Delete")) {
                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine("Are you sure you want to delete the following mods?");
                    messageBuilder.AppendLine();

                    foreach(string assembly in selectedAssemblyDefinitions.Select(assembly => Path.GetFileNameWithoutExtension(assembly.Name))) {
                        messageBuilder.AppendLine($"- {assembly}");
                    }

                    bool result = EditorUtility.DisplayDialog("Confirm Deletion", messageBuilder.ToString(), "Yes", "Cancel");
                    if(result) {
                        foreach(var assembly in selectedAssemblyDefinitions) {
                            MessageBus.SendMessage("RemoveFolderMappings", Path.GetFileNameWithoutExtension(assembly.Name));
                            Directory.GetParent(assembly.AssemblyPath).Delete(true);
                            selectedMods.Remove(assembly.AssemblyPath);
                        }

                        AssetDatabase.Refresh();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void CreateModList() {
            var modKeys = selectedMods.Keys.ToList(); // Capture keys in a list to maintain indexing
            modAssemblyList = new ReorderableList(modKeys, typeof(string), false, true, false, false);

            modAssemblyList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Mods");
            };

            modAssemblyList.drawElementCallback = (rect, index, isActive, isFocused) => {
                if(index < 0 || index >= modKeys.Count)
                    return;

                var modKey = modKeys[index]; // Ensure we use the correct key
                var modName = Path.GetFileNameWithoutExtension(modKey); // Remove path for cleaner display
                var toggleValue = selectedMods[modKey]; // Correctly reference the dictionary key

                var labelRect = new Rect(rect.x, rect.y + 2, rect.width - 40, EditorGUIUtility.singleLineHeight);
                var toggleRect = new Rect(rect.x + rect.width - 15, rect.y + 2, 20, EditorGUIUtility.singleLineHeight);
                var buttonRect = new Rect(rect.x + rect.width - 70, rect.y + 2, 50, EditorGUIUtility.singleLineHeight);

                EditorGUI.LabelField(labelRect, modName, EditorStyles.boldLabel);
                selectedMods[modKey] = EditorGUI.Toggle(toggleRect, toggleValue);
                if(GUI.Button(buttonRect, "Focus")) {
                    AssemblyDefinitionAsset loadedAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(modKey);
                    EditorGUIUtility.PingObject(loadedAsset);
                    Selection.activeObject = loadedAsset;
                }
            };
        }
    }
}
