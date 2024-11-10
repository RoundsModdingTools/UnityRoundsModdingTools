using BepInEx;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;
using UnityRoundsModdingTools.Editor.Utils;
using UnityRoundsModdingTools.Editor.Utils.Template;

namespace UnityRoundsModdingTools.Editor.Windows {
    public class CreateModWindow : EditorWindow {
        private static readonly Regex modIdRegex = new Regex("^[a-zA-Z0-9_.]*$", RegexOptions.Compiled);

        private string modId;
        private string modName;
        private string modInitial;

        private static List<AssemblyDefinition> assemblyReferences;
        private static List<string> includedPrecompiledAssemblies = new List<string>() {
            "0Harmony.dll", "BepInEx.dll", "Assembly-CSharp.dll"
        };

        private static ReorderableList assemblyReferencesList;
        private static ReorderableList precompiledAssembliesList;

        [MenuItem("Unity Rounds Modding Tools/Create Mod")]
        private static void ShowWindow() {
            GetWindow(typeof(CreateModWindow), false, "Create Mod");
        }

        private void OnEnable() {
            if(assemblyReferences == null) {
                assemblyReferences = AssemblyDefinition.All
                    .Where(x => x.Name == "UnboundLib" || x.Name == "ModdingUtils" || x.Name == "CardChoiceSpawnUniqueCardPatch")
                    .ToList();
                assemblyReferencesList = CreateAssemblyReferencesList();
                precompiledAssembliesList = CreatePrecompiledAssembliesList();
            }
        }

        private void OnGUI() {
            GUIUtils.DrawTitle("Create Mod");
            GUILayout.Label("Enter the mod details.", EditorStyles.boldLabel);

            GUIUtils.CreateTextInput("Mod ID:", ref modId);
            if(!modId.IsNullOrWhitespace() && !modIdRegex.IsMatch(modId))
                EditorGUILayout.HelpBox("Invalid mod ID. Only lowercase letters, numbers, underscores, and periods are allowed.", MessageType.Error);

            GUIUtils.CreateTextInput("Mod Name:", ref modName);
            if(!modName.IsNullOrWhitespace() && Path.GetInvalidFileNameChars().Any(x => modName.Contains(x)))
                EditorGUILayout.HelpBox("Invalid mod name. The name cannot contain any of the following characters: / \\ ? * : \" | < >", MessageType.Error);
            else if(AssemblyDefinition.All.Any(x => x.Name == modName))
                EditorGUILayout.HelpBox("Mod with this name already exists.", MessageType.Error);

            GUIUtils.CreateTextInput("Mod Initials:", ref modInitial);

            GUILayout.Space(10);
            GUILayout.Label("Select the assemblies to include in the mod.", EditorStyles.boldLabel);
            assemblyReferencesList.DoLayoutList();

            GUILayout.Space(10);
            precompiledAssembliesList.DoLayoutList();

            bool canCreateMod = !modId.IsNullOrWhitespace()
                && modIdRegex.IsMatch(modId)
                && !modName.IsNullOrWhitespace()
                && !Path.GetInvalidFileNameChars().Any(x => modName.Contains(x))
                && !modInitial.IsNullOrWhitespace()
                && !AssemblyDefinition.All.Any(x => x.Name == modName);

            bool isGithubUrl = Settings.Instance.ModTemplatePath.StartsWith("https://github.com/");
            if(isGithubUrl) {
                if(!GithubUtils.IsValidGithubUrl(Settings.Instance.ModTemplatePath)) {
                    EditorGUILayout.HelpBox("Invalid GitHub URL.", MessageType.Error);
                    canCreateMod = false;
                }
            } else {
                if(!Directory.Exists(Settings.Instance.ModTemplatePath)) {
                    EditorGUILayout.HelpBox("Path not found.", MessageType.Error);
                    canCreateMod = false;
                }
            }

            GUI.enabled = canCreateMod;
            if(GUILayout.Button("Create Mod")) {
                string modSafeName = modName.Replace(" ", "").Replace("-", "");
                string modPath = Path.Combine(Settings.Instance.ModTemplateOutputPath, $"_{modName}");
                string modTemplatePath = Settings.Instance.ModTemplatePath;
                modTemplatePath = isGithubUrl ? Path.Combine(Settings.Instance.TempPath, "ModTemplate") : modTemplatePath;

                AssetDatabase.StartAssetEditing();

                try {
                    if(isGithubUrl) {
                        string savePath = GithubUtils.DownloadGithubProject(Settings.Instance.ModTemplatePath, "ModTemplate");
                        FileSystemManager.CopyDirectory(savePath, modPath);
                        Directory.Delete(Settings.Instance.TempPath, true);
                    } else {
                        FileSystemManager.CopyDirectory(modTemplatePath, modPath);
                    }

                    // Create the assembly definition
                    var assemblyDefinition = new AssemblyDefinition(modSafeName);
                    assemblyDefinition.References = assemblyReferences.Select(x => x.Name).ToList();
                    assemblyDefinition.PrecompiledReferences = includedPrecompiledAssemblies;
                    assemblyDefinition.AssemblyPath = Path.Combine(modPath, $"{modSafeName}.asmdef");
                    assemblyDefinition.Save();

                    string[] modIds = GetModIds(assemblyReferences);
                    TemplateManager.ApplyTemplateToFiles(modPath, modPath, "mod", modId, modName, modInitial, "1.0.0", modIds, modPath);
                } catch(Exception e) {
                    Debug.LogError($"Failed to create mod: {e}");
                }
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }


            GUILayout.Space(10);
            GUI.enabled = true;
        }

        private string[] GetModIds(List<AssemblyDefinition> assemblies) {
            return assemblies
                .Select(x => x.Assembly)
                .Where(assembly => assembly != null)
                .SelectMany(x => x.GetTypes())
                .SelectMany(type => type.GetCustomAttributes<BepInPlugin>())
                .Select(attribute => attribute.GUID)
                .ToArray();
        }

        private ReorderableList CreateAssemblyReferencesList() {
            ReorderableList reorderableList = new ReorderableList(assemblyReferences, typeof(AssemblyDefinition), true, true, true, true);
            reorderableList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Assembly References");
            };

            reorderableList.drawElementCallback = (rect, index, active, focused) => {
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                AssemblyDefinitionAsset selectedAsset = assemblyReferences[index] != null
                    ? AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assemblyReferences[index].AssemblyPath)
                    : null;
                selectedAsset = (AssemblyDefinitionAsset)EditorGUI.ObjectField(rect, selectedAsset, typeof(AssemblyDefinitionAsset), false);

                assemblyReferences[index] = selectedAsset != null ? AssemblyDefinition.LoadFromAssemblyDefinitionAsset(selectedAsset) : null;
            };

            reorderableList.onAddCallback = list => {
                assemblyReferences.Add(null);
            };

            reorderableList.onRemoveCallback = list => {
                assemblyReferences.RemoveAt(list.index);
            };

            return reorderableList;
        }

        private ReorderableList CreatePrecompiledAssembliesList() {
            ReorderableList reorderableList = new ReorderableList(includedPrecompiledAssemblies, typeof(string), true, true, true, true);
            reorderableList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Precompiled Assemblies");
            };

            reorderableList.drawElementCallback = (rect, index, active, focused) => {
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                List<string> availablePrecompiledAssemblies = CompilationPipeline.GetPrecompiledAssemblyNames()
                    .OrderBy(x => x)
                    .ToList();

                List<GUIContent> availablePrecompiledAssembliesContent = availablePrecompiledAssemblies.Select(x => new GUIContent(x)).ToList();

                int selectedIndex = availablePrecompiledAssemblies.IndexOf(includedPrecompiledAssemblies[index]);
                if(selectedIndex == -1) {
                    availablePrecompiledAssembliesContent.Insert(0, new GUIContent("None"));
                    selectedIndex = 0;
                }

                includedPrecompiledAssemblies[index] = availablePrecompiledAssembliesContent[EditorGUI.Popup(rect, selectedIndex, availablePrecompiledAssembliesContent.ToArray())].text;
            };

            reorderableList.onAddCallback = (list) => {
                includedPrecompiledAssemblies.Add(null);
            };

            reorderableList.onRemoveCallback = (list) => {
                includedPrecompiledAssemblies.RemoveAt(list.index);
            };

            return reorderableList;
        }
    }
}
