using System.IO;
using UnityEditor;
using UnityEngine;
using URMT.Core;
using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;
using URMT.General;

namespace URMT.ModManager {
    [URMTModule("ModManager", "com.aalund13.urmt.modmanager")]
    [URMTModuleDependency("com.aalund13.urmt.core")]
    [URMTModuleDependency("com.aalund13.urmt.general")]
    public class ModManagerModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[0];

        public void OnModuleLoad() {
            LoggerUtils.Log("ModManager Module Loaded");
        }

        public static void ConvertToUnityProject(string path, string modName) {
            string[] solutionFiles = Directory.GetFiles(path, "*.sln", SearchOption.AllDirectories);
            string[] assemblyDefinitionFiles = Directory.GetFiles(path, "*.asmdef", SearchOption.AllDirectories);
            AssemblyDefinition[] assemblyDefinitions = new AssemblyDefinition[0];

            if(solutionFiles.Length > 0) {
                Solution solution = new Solution(solutionFiles[0]);
                if(solutionFiles.Length > 0 && solution.Projects.Length == 1) {
                    assemblyDefinitions = solution.ConvertToAssemblyDefinitions(CoreModule.Instance.ModsFolderPath);
                } else if(solutionFiles.Length > 0 && solution.Projects.Length > 1) {
                    assemblyDefinitions = solution.ConvertToAssemblyDefinitions(Path.Combine(CoreModule.Instance.ModsFolderPath, Path.GetFileNameWithoutExtension(solutionFiles[0])));
                }
            } else if(assemblyDefinitionFiles.Length > 0) {
                FileSystemUtils.CopyDirectory(path, Path.Combine(CoreModule.Instance.ModsFolderPath, modName));

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