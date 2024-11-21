using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;
using UnityRoundsModdingTools.Editor.Utils;

namespace UnityRoundsModdingTools.Editor.Windows {
    public class InstallModWindow : EditorWindow {
        private string selectedSolutionPath;

        [MenuItem("Unity Rounds Modding Tools/Install Mod")]
        private static void ShowWindow() {
            GetWindow(typeof(InstallModWindow), false, "Install Mod");
        }

        private void OnEnable() {
            selectedSolutionPath = EditorPrefs.GetString("SelectedSolutionPath");
        }

        private void OnGUI() {
            EditorPrefs.SetString("SelectedSolutionPath", selectedSolutionPath);

            GUIUtils.DrawTitle("Install Mod");

            GUILayout.Label("Enter a GitHub repository URL or a local solution directory path.", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mod Solution Path:", GUILayout.Width(120));
            selectedSolutionPath = EditorGUILayout.TextField(selectedSolutionPath);

            if(GUILayout.Button("Browse")) {
                string directory = (!string.IsNullOrEmpty(selectedSolutionPath) && Directory.Exists(Path.GetDirectoryName(selectedSolutionPath))) ? selectedSolutionPath : Application.dataPath; // Start from the Assets folder
                string newSelectedSolutionPath = EditorUtility.OpenFolderPanel("Select a Mod Directory", directory, "");
                selectedSolutionPath = string.IsNullOrEmpty(newSelectedSolutionPath) ? selectedSolutionPath : newSelectedSolutionPath;

                GUI.FocusControl(null);
                Repaint();
            }
            GUILayout.EndHorizontal();

            if(!string.IsNullOrEmpty(selectedSolutionPath)) {
                bool isGithubUrl = selectedSolutionPath.StartsWith("https://github.com/");

                if(isGithubUrl) {
                    if(!GithubUtils.IsValidGithubUrl(selectedSolutionPath)) {
                        EditorGUILayout.HelpBox("Invalid GitHub URL.", MessageType.Error);
                        GUI.enabled = false;
                    }
                } else {
                    if(!Directory.Exists(selectedSolutionPath)) {
                        EditorGUILayout.HelpBox("Solution directory not found.", MessageType.Error);
                        GUI.enabled = false;
                    }
                }

                if(GUILayout.Button("Install Mod")) {
                    List<string> installAssemblies;
                    if(isGithubUrl) installAssemblies = GithubUtils.InstallGithubProject(selectedSolutionPath);
                    else installAssemblies = ProjectUtils.ConvertToUnityProject(selectedSolutionPath);

                    if(installAssemblies != null) {
                        foreach(string installAssembly in installAssemblies) {
                            if(!ProjectMappings.Instance.FolderMappings.Exists(folder => folder.AssemblyName == installAssembly)) {
                                ProjectMappings.Instance.FolderMappings.Add(new FolderMapping(installAssembly, "Libraries"));
                            }
                        }
                        ProjectMappings.Save();
                        AssetDatabase.Refresh();
                    }
                }
                GUI.enabled = true;
            }
        }
    }
}