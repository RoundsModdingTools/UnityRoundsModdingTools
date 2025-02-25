using GitHubAPI;
using System.IO;
using System.Text.RegularExpressions;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using URMT.Core.UI;
using URMT.Core;
using URMT.Template.Templates;
using UnityEditor.Compilation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using URMT.Core.Utils;
using System.IO.Compression;
using URMT.Template;

namespace URMT.Windows {
    public class CreateModWindow : EditorWindow {
        private static readonly Regex modIdRegex = new Regex("^[a-zA-Z0-9_.]*$", RegexOptions.Compiled);

        private static string modId;
        private static string modName;
        private static string modInitial;

        private static List<AssemblyDefinition> assemblyReferences;
        private static List<string> includedPrecompiledAssemblies = new List<string>() {
            "0Harmony.dll", "BepInEx.dll", "Assembly-CSharp.dll"
        };

        private static ReorderableList assemblyReferencesList;
        private static ReorderableList precompiledAssembliesList;

        [MenuItem("URMT/Mod Manager/Create Mod")]
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

            // Check if the mod ID is valid
            modId = EditorGUILayout.TextField("Mod ID:", modId);
            if(!string.IsNullOrWhiteSpace(modId) && !modIdRegex.IsMatch(modId))
                EditorGUILayout.HelpBox("Invalid mod ID. Only lowercase letters, numbers, underscores, and periods are allowed.", MessageType.Error);

            // Check if the mod name is valid
            modName = EditorGUILayout.TextField("Mod Name:", modName);
            if(!string.IsNullOrWhiteSpace(modName) && Path.GetInvalidFileNameChars().Any(x => modName.Contains(x)))
                EditorGUILayout.HelpBox("Invalid mod name. The name cannot contain any of the following characters: / \\ ? * : \" | < >", MessageType.Error);
            else if(AssemblyDefinition.All.Any(x => x.Name == modName))
                EditorGUILayout.HelpBox("Mod with this name already exists.", MessageType.Error);

            modInitial = EditorGUILayout.TextField("Mod Initials:", modInitial);

            GUILayout.Space(10);
            GUILayout.Label("Select the assemblies to include in the mod.", EditorStyles.boldLabel);
            assemblyReferencesList.DoLayoutList();

            GUILayout.Space(10);
            precompiledAssembliesList.DoLayoutList();

            string modTemplatePath = TemplateModuleSettings.Instance.ModTemplateImportPath;

            // Check if the we can create the mod
            bool canCreateMod = !string.IsNullOrWhiteSpace(modId)
                && modIdRegex.IsMatch(modId)
                && !string.IsNullOrWhiteSpace(modName)
                && !Path.GetInvalidFileNameChars().Any(x => modName.Contains(x))
                && !string.IsNullOrWhiteSpace(modInitial)
                && !AssemblyDefinition.All.Any(x => x.Name == modName);

            bool isGithubUrl = modTemplatePath.StartsWith("https://github.com/");
            if(isGithubUrl) {
                if(!GithubUtils.IsValidGithubUrl(TemplateModuleSettings.Instance.ModTemplateImportPath)) {
                    EditorGUILayout.HelpBox("Invalid GitHub URL.", MessageType.Error);
                    canCreateMod = false;
                }
            } else {
                if(!Directory.Exists(TemplateModuleSettings.Instance.ModTemplateImportPath)) {
                    EditorGUILayout.HelpBox("Path not found.", MessageType.Error);
                    canCreateMod = false;
                }
            }

            GUI.enabled = canCreateMod;
            if(GUILayout.Button("Create Mod")) {
                string modSafeName = modName.Replace(" ", "").Replace("-", "");
                string modPath = Path.Combine(TemplateModuleSettings.Instance.ModTemplateExportPath, $"_{modName}");
                string tempSavePath = Path.Combine(CoreModule.Instance.TempPath, "Templates");

                AssetDatabase.StartAssetEditing();

                try {
                    if(isGithubUrl) {
                        using(GitHubClient client = new GitHubClient()) {
                            (string owner, string repo) = GithubUtils.ExtractOwnerAndRepo(modTemplatePath);

                            client.DownloadGithubZip(Path.Combine(tempSavePath, "ModTemplate.zip"), owner, repo);
                        }

                        ZipFile.ExtractToDirectory(Path.Combine(tempSavePath, "ModTemplate.zip"), tempSavePath);
                        string firstDirectoryPath = Directory.GetDirectories(tempSavePath)[0];

                        FileSystemUtils.CopyDirectory(firstDirectoryPath, modPath);

                        Directory.Delete(tempSavePath, true);
                    } else {
                        FileSystemUtils.CopyDirectory(modTemplatePath, modPath);
                    }

                    // Create the assembly definition
                    var assemblyDefinition = new AssemblyDefinition(modSafeName);
                    assemblyDefinition.References = assemblyReferences.Select(x => x.Name).ToList();
                    assemblyDefinition.PrecompiledReferences = includedPrecompiledAssemblies;
                    assemblyDefinition.AssemblyPath = Path.Combine(modPath, $"{modSafeName}.asmdef");
                    assemblyDefinition.Save();

                    string[] modIds = GetModIds(assemblyReferences);
                    TemplateManager.ApplyTemplateToFiles(modPath, modPath, "ModTemplate", modId, modName, modInitial, "1.0.0", modIds, modPath);
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
