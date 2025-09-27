using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using URMT.Core.ScriptableObjects;
using URMT.Core.Settings;
using URMT.Core.UI;
using URMT.General.Entities;

namespace URMT.General {
    public class GeneralModuleSettings : SettingsSingleton<GeneralModuleSettings> {
        public override string Name => "General Module Settings";

        private static ReorderableList modBundleMappingsList;
        private static ReorderableList folderMappingsList;


        public List<ModBundleMapping> ModBundleMappings = new List<ModBundleMapping>();
        public List<FolderMapping> FolderMappings = new List<FolderMapping>(GetURMTFolderMappings()) {
            new FolderMapping("ThunderstoreAPI", "URMT Wrappers"),
            new FolderMapping("GitHubAPI", "URMT Wrappers"),
            new FolderMapping("CardChoiceSpawnUniqueCardPatch", "Libraries"),
            new FolderMapping("CardThemeLib", "Libraries"),
            new FolderMapping("ClassesManagerReborn", "Libraries"),
            new FolderMapping("ItemShops", "Libraries"),
            new FolderMapping("ModdingUtils", "Libraries"),
            new FolderMapping("ModsPlus", "Libraries"),
            new FolderMapping("PickNCards", "Libraries"),
            new FolderMapping("RarityLib", "Libraries"),
            new FolderMapping("RoundsWithFriends", "Libraries"),
            new FolderMapping("UnboundLib", "Libraries"),
            new FolderMapping("WillsWackyManagers", "Libraries"),
            new FolderMapping("ILGenerator", "Libraries"),
        };

        private static List<FolderMapping> GetURMTFolderMappings() {
            // Get all assemblies that are in the URMT namespace
            var urmtAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.FullName.Contains("URMT"))
                .Select(assembly => assembly.GetName().Name)
                .ToList();

            return urmtAssemblies.Select(assembly => new FolderMapping(assembly, "URMT")).ToList();
        }

        public void OnEnable() {
            var serializedObject = new SerializedObject(this);

            CreateProjectMappingsList(serializedObject.FindProperty("ModBundleMappings"));
            CreateFolderMappingsList(serializedObject.FindProperty("FolderMappings"));
        }

        [RenderFor(nameof(ModBundleMappings))]
        private void RenderModBundleMappings(SerializedProperty serializedProperty) {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Mappings", EditorStyles.boldLabel);

            GUIUtils.RenderIndented(() => {

                if(File.Exists("Assets/Editor/CsprojPostprocessor.cs")) GUI.enabled = false;
                modBundleMappingsList.DoLayoutList();
                GUI.enabled = true;
            });
        }

        [RenderFor(nameof(FolderMappings))]
        private void RenderFolderMappings(SerializedProperty serializedProperty) {
            GUIUtils.RenderIndented(() => {
                if(File.Exists("Assets/Editor/CsprojPostprocessor.cs")) GUI.enabled = false;
                folderMappingsList.DoLayoutList();
                GUI.enabled = true;

                if(GUILayout.Button("Recompile")) {
                    var editorAssembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                    var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
                    var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
                    dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);

                    var SyncVSType = editorAssembly.GetType("UnityEditor.SyncVS");
                    var SyncSolutionMethod = SyncVSType.GetMethod("SyncIfFirstFileOpenSinceDomainLoad", BindingFlags.Static | BindingFlags.Public);
                    SyncSolutionMethod.Invoke(editorCompilationInterfaceType, null);
                }
            });
        }

        private void CreateProjectMappingsList(SerializedProperty projectMappingsProperty) {
            modBundleMappingsList = new ReorderableList(projectMappingsProperty.serializedObject, projectMappingsProperty, true, true, true, true);
            modBundleMappingsList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Mod Bundle Mappings");
            };
            modBundleMappingsList.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = modBundleMappingsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                float halfWidth = rect.width / 2 - 10;

                // Get the current ModName
                SerializedProperty modNameProperty = element.FindPropertyRelative("ModName");
                GUIUtils.DrawAssemblyDefinitionProperty(modNameProperty, rect, rect.width / 2 - 10);

                // AssetBundleName field
                SerializedProperty modAssetProperty = element.FindPropertyRelative("AssetBundleName");
                GUIUtils.DrawAssetBundleProperty(modAssetProperty, new Rect(rect.x + halfWidth + 10, rect.y, halfWidth, EditorGUIUtility.singleLineHeight));

                projectMappingsProperty.serializedObject.ApplyModifiedProperties();
            };
            modBundleMappingsList.onAddCallback = list => {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;

                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("ModName").stringValue = "";
                element.FindPropertyRelative("AssetBundleName").stringValue = "";

                projectMappingsProperty.serializedObject.ApplyModifiedProperties();
            };
            modBundleMappingsList.onRemoveCallback = list => {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                projectMappingsProperty.serializedObject.ApplyModifiedProperties();
            };
        }

        private void CreateFolderMappingsList(SerializedProperty folderMappingsProperty) {
            folderMappingsList = new ReorderableList(folderMappingsProperty.serializedObject, folderMappingsProperty, true, true, true, true);
            folderMappingsList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Folder Mappings");
            };
            folderMappingsList.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = folderMappingsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                float halfWidth = rect.width / 2 - 10;

                // Get the current AssemblyName
                SerializedProperty assemblyNameProperty = element.FindPropertyRelative("AssemblyName");
                GUIUtils.DrawAssemblyDefinitionProperty(assemblyNameProperty, rect, rect.width / 2 - 10);

                // FolderName field
                EditorGUI.PropertyField(
                    new Rect(rect.x + halfWidth + 10, rect.y, halfWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("FolderName"),
                    new GUIContent("Folder Name")
                );

                folderMappingsProperty.serializedObject.ApplyModifiedProperties();
            };
            folderMappingsList.onAddCallback = list => {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;

                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("AssemblyName").stringValue = "";
                element.FindPropertyRelative("FolderName").stringValue = "";

                folderMappingsProperty.serializedObject.ApplyModifiedProperties();
            };
            folderMappingsList.onRemoveCallback = list => {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                folderMappingsProperty.serializedObject.ApplyModifiedProperties();
            };
        }
    }
}