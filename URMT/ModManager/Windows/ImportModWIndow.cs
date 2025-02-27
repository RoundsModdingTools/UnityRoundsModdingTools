using GitHubAPI;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;
using URMT.Core;
using URMT.Core.Managers;
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
                        string tempPath = Path.Combine(CoreModule.Instance.TempPath, $"{selectedOwner}-{selectedRepo}");

                        using(GitHubClient client = new GitHubClient()) {
                            client.DownloadGithubZip($"{tempPath}.zip", selectedOwner, selectedRepo);

                            ZipFile.ExtractToDirectory($"{tempPath}.zip", Path.Combine(CoreModule.Instance.TempPath, $"{selectedOwner}-{selectedRepo}"));
                            string firstDirectoryPath = Directory.GetDirectories(tempPath)[0];

                            ConvertToUnityProject(firstDirectoryPath);

                            File.Delete($"{tempPath}.zip");
                            Directory.Delete(tempPath, true);
                        }
                    } else {
                        ConvertToUnityProject(modPath);
                    }
                }
                GUI.enabled = true;
            }
        }

        private void ConvertToUnityProject(string path) {
            string[] solutionFiles = Directory.GetFiles(path, "*.sln", SearchOption.AllDirectories);
            string[] assemblyDefinitionFiles = Directory.GetFiles(path, "*.asmdef", SearchOption.AllDirectories);
            AssemblyDefinition[] assemblyDefinitions;

            if(solutionFiles.Length > 0) {
                Solution solution = new Solution(solutionFiles[0]);
                if(solution.Projects.Length == 1) {
                    assemblyDefinitions = solution.ConvertToAssemblyDefinitions(CoreModule.Instance.ModsFolderPath);
                } else if(solution.Projects.Length > 1) {
                    assemblyDefinitions = solution.ConvertToAssemblyDefinitions(Path.Combine(CoreModule.Instance.ModsFolderPath, Path.GetFileNameWithoutExtension(solutionFiles[0])));
                } else if(assemblyDefinitionFiles.Length > 0) {
                    FileSystemUtils.CopyDirectory(path, CoreModule.Instance.ModsFolderPath);

                    assemblyDefinitions = new AssemblyDefinition[assemblyDefinitionFiles.Length];
                    for(int i = 0; i < assemblyDefinitionFiles.Length; i++) {
                        assemblyDefinitions[i] = AssemblyDefinition.Load(assemblyDefinitionFiles[i]);
                    }
                } else {
                    Debug.LogError("Failed to find a solution or assembly definition file in the selected directory.");
                    return;
                }

                foreach(var assembly in assemblyDefinitions) {
                    GeneralModule.AddFolderMapping(assembly.Name, "Libraries");
                }

                AssetDatabase.Refresh();
            }
        }
    }
}
