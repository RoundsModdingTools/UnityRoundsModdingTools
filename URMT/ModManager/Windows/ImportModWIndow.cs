using GitHubAPI;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;
using URMT.Core;
using URMT.Core.UI;
using URMT.Core.Utils;
using URMT.General;

namespace URMT.ModManager.Windows {
    public class ImportModWIndow : EditorWindow {
        private string modPath;

        [MenuItem("URMT/Mod Manager/Import Mod")]
        private static void ShowWindow() {
            GetWindow<ImportModWIndow>("Import Mod");
        }

        private void OnGUI() {
            GUIUtils.DrawTitle("Import Mod");

            GUILayout.Label("Enter a GitHub repository URL or a local solution directory path.", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            modPath = EditorGUILayout.TextField("Mod Path", modPath);

            if(GUILayout.Button("Browse", GUILayout.Width(60))) {
                string directory = (!string.IsNullOrEmpty(modPath) && Directory.Exists(Path.GetDirectoryName(modPath))) ? modPath : Application.dataPath; // Start from the Assets folder
                string newSelectedSolutionPath = EditorUtility.OpenFolderPanel("Select a Mod Directory", directory, "");
                modPath = string.IsNullOrEmpty(newSelectedSolutionPath) ? modPath : newSelectedSolutionPath;

                GUI.FocusControl(null);
                Repaint();
            }
            GUILayout.EndHorizontal();

            if(!string.IsNullOrEmpty(modPath)) {
                (string selectedOwner, string selectedRepo) = modPath.StartsWith("https://github.com/") ? GithubUtils.ExtractOwnerAndRepo(modPath) : (null, null);

                if(selectedOwner != null) {
                    if(!GithubUtils.IsValidGithubUrl(modPath)) {
                        EditorGUILayout.HelpBox("Invalid GitHub URL.", MessageType.Error);
                        GUI.enabled = false;
                    }
                } else {
                    if(!Directory.Exists(modPath)) {
                        EditorGUILayout.HelpBox("Solution directory not found.", MessageType.Error);
                        GUI.enabled = false;
                    }
                }

                if(GUILayout.Button("Import Mod")) {
                    if(selectedOwner != null) {
                        string tempPath = Path.Combine(CoreModule.Instance.TempPath, "ImportMod", $"{selectedOwner}-{selectedRepo}");
                        string filePath = Path.Combine(tempPath, $"{selectedOwner}-{selectedRepo}.zip");

                        using(GitHubClient client = new GitHubClient()) {
                            client.DownloadGithubZip(filePath, selectedOwner, selectedRepo);

                            ZipFile.ExtractToDirectory(filePath, tempPath);
                            string firstDirectoryPath = Directory.GetDirectories(tempPath)[0];

                            ModManagerModule.ConvertToUnityProject(firstDirectoryPath, selectedRepo);

                            Directory.Delete(tempPath, true);
                        }
                    } else {
                        ModManagerModule.ConvertToUnityProject(modPath, selectedRepo);
                    }
                }
                GUI.enabled = true;
            }
        }
    }
}
