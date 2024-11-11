using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;
using UnityRoundsModdingTools.Editor.Utils;

namespace Assets.Plugins.UnityRoundsModdingTools.Editor.Windows {
    public class ToolSettingsWindow : EditorWindow {
        private static Vector2 scrollPosition;

        [MenuItem("Unity Rounds Modding Tools/Settings")]
        private static void ShowWindow() {
            GetWindow(typeof(ToolSettingsWindow), false, "Settings");
        }

        private void OnGUI() {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(EditorGUIUtility.currentViewWidth), GUILayout.Height(Screen.height - 30));

            GUIUtils.DrawTitle("Unity Rounds Modding Tools Settings");
            SettingsGUI.Instance.RenderSettings();

            EditorGUILayout.EndScrollView();
        }
    }

    public class SettingsGUI {
        private static ReorderableList projectMappingsList;
        private static ReorderableList folderMappingsList;

        private static SerializedObject settingsSerializedObject;
        private static SerializedProperty projectMappingsProperty;
        private static SerializedProperty folderMappingsProperty;

        private static SettingsGUI instance;
        public static SettingsGUI Instance {
            get {
                if(instance == null) {
                    instance = new SettingsGUI();
                    instance.InitializeGUI();
                }
                return instance;
            }
        }

        public void RenderSettings() {
            GUILayout.Label("Path Settings", EditorStyles.boldLabel);
            Settings.Instance.DllsFolderPath = EditorGUILayout.TextField("DLLs Folder Path", Settings.Instance.DllsFolderPath);
            Settings.Instance.ModsFolderPath = EditorGUILayout.TextField("Mods Folder Path", Settings.Instance.ModsFolderPath);
            Settings.Instance.BepinexFolderPath = EditorGUILayout.TextField("BepInEx Folder Path", Settings.Instance.BepinexFolderPath);
            Settings.Instance.TempPath = EditorGUILayout.TextField("Temp Path", Settings.Instance.TempPath);

            GUILayout.Space(10);
            GUILayout.Label("Mod Template Settings", EditorStyles.boldLabel);
            Settings.Instance.TemplatePath = EditorGUILayout.TextField("Template Path", Settings.Instance.TemplatePath);
            Settings.Instance.TemplateOutputPath = EditorGUILayout.TextField("Template Output Path", Settings.Instance.TemplateOutputPath);

            GUILayout.Space(10);
            GUILayout.Label("Mappings", EditorStyles.boldLabel);
            projectMappingsList.DoLayoutList();
            GUILayout.Space(10);
            folderMappingsList.DoLayoutList();

            GUILayout.Space(10);
            if(GUILayout.Button("Recompile")) {
                var editorAssembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
                var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
                dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);

                var SyncVSType = editorAssembly.GetType("UnityEditor.SyncVS");
                var SyncSolutionMethod = SyncVSType.GetMethod("SyncIfFirstFileOpenSinceDomainLoad", BindingFlags.Static | BindingFlags.Public);
                SyncSolutionMethod.Invoke(editorCompilationInterfaceType, null);
            }
        }

        public void InitializeGUI() {
            settingsSerializedObject = new SerializedObject(ProjectMappings.Instance);
            projectMappingsProperty = settingsSerializedObject.FindProperty("projectMappings");
            folderMappingsProperty = settingsSerializedObject.FindProperty("folderMappings");

            CreateProjectMappingsList();
            CreateFolderMappingsList();
        }

        private void CreateProjectMappingsList() {
            projectMappingsList = new ReorderableList(settingsSerializedObject, projectMappingsProperty, true, true, true, true);
            projectMappingsList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Project Mappings");
            };
            projectMappingsList.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = projectMappingsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                float halfWidth = rect.width / 2 - 10;

                // Get the current ModName
                SerializedProperty modNameProperty = element.FindPropertyRelative("ModName");
                DrawAssemblyDefinitionProperty(modNameProperty, rect);
                
                // AssetBundleName field
                EditorGUI.PropertyField(
                    new Rect(rect.x + halfWidth + 10, rect.y, halfWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("AssetBundleName"),
                    new GUIContent("Asset Bundle Name")
                );

                settingsSerializedObject.ApplyModifiedProperties();
            };
            projectMappingsList.onAddCallback = list => {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;

                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("ModName").stringValue = "";
                element.FindPropertyRelative("AssetBundleName").stringValue = "";

                settingsSerializedObject.ApplyModifiedProperties();
            };
            projectMappingsList.onRemoveCallback = list => {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                settingsSerializedObject.ApplyModifiedProperties();
            };
        }

        private void CreateFolderMappingsList() {
            folderMappingsList = new ReorderableList(settingsSerializedObject, folderMappingsProperty, true, true, true, true);
            folderMappingsList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Folder Mappings");
            };
            folderMappingsList.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = folderMappingsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                float halfWidth = rect.width / 2 - 10;

                // Get the current AssemblyName
                SerializedProperty assemblyNameProperty = element.FindPropertyRelative("AssemblyName");
                DrawAssemblyDefinitionProperty(assemblyNameProperty, rect);

                // FolderName field
                EditorGUI.PropertyField(
                    new Rect(rect.x + halfWidth + 10, rect.y, halfWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("FolderName"),
                    new GUIContent("Folder Name")
                );

                settingsSerializedObject.ApplyModifiedProperties();
            };
            folderMappingsList.onAddCallback = list => {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;

                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("AssemblyName").stringValue = "";
                element.FindPropertyRelative("FolderName").stringValue = "";

                settingsSerializedObject.ApplyModifiedProperties();
            };
            folderMappingsList.onRemoveCallback = list => {
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                settingsSerializedObject.ApplyModifiedProperties();
            };
        }

        private void DrawAssemblyDefinitionProperty(SerializedProperty property, Rect rect) {
            string currentAssemblyName = property.stringValue;
            float halfWidth = rect.width / 2 - 10;

            // Find the current assembly definition
            AssemblyDefinition currentAssemblyDefinition = AssemblyDefinition.All.FirstOrDefault(x => x.Name == currentAssemblyName);
            AssemblyDefinitionAsset currentAssemblyAsset = currentAssemblyDefinition != null ? AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(currentAssemblyDefinition.AssemblyPath) : null;

            // Create ObjectField for selecting AssemblyDefinitionAsset
            AssemblyDefinitionAsset newAssemblyAsset = (AssemblyDefinitionAsset)EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, halfWidth, EditorGUIUtility.singleLineHeight),
                currentAssemblyAsset,
                typeof(AssemblyDefinitionAsset),
                false
            );

            // Update AssemblyName if a new assembly is selected
            if(newAssemblyAsset != null && newAssemblyAsset != currentAssemblyAsset) {
                property.stringValue = AssemblyDefinition.LoadFromAssemblyDefinitionAsset(newAssemblyAsset).Name;
            }
        }
    }
}
