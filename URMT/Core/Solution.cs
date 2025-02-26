using System;
using System.IO;
using URMT.Core.Utils;

namespace URMT.Core {
    public class Solution {
        public static readonly Solution UnitySolution = new Solution(Directory.GetCurrentDirectory());

        private static string[] BlacklistedDirectory = new string[] {
            "obj",
            "bin",
            ".vs",
            "Assemblies"
        };
        private static string[] blacklistedFileExtension = new string[] {
            "csproj",
            "sln",
            "dll"
        };

        public CSProj[] Projects { get; private set; }

        public Solution(string path) {
            if(path == null) throw new ArgumentNullException(nameof(path));
            if(File.Exists(path)) path = Path.GetDirectoryName(path);
            if(!Directory.Exists(path)) throw new DirectoryNotFoundException($"Directory not found at path: {path}");

            string[] csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
            Projects = new CSProj[csprojFiles.Length];

            for(int i = 0; i < csprojFiles.Length; i++) {
                Projects[i] = new CSProj(csprojFiles[i]);
            }
        }

        public AssemblyDefinition[] ConvertToAssemblyDefinitions(string outputPath) {
            AssemblyDefinition[] assemblyDefinitions = new AssemblyDefinition[Projects.Length];

            for(int i = 0; i < Projects.Length; i++) {
                CSProj project = Projects[i];

                string assemblyName = project.Name;
                string assemblyDirectory = Path.Combine(outputPath, assemblyName);
                string assemblyPath = Path.Combine(assemblyDirectory, assemblyName + ".asmdef");

                if(assemblyName == null) {
                    UnityEngine.Debug.LogError($"Failed to get assembly name for project at path: {project.Path}");
                    continue;
                }

                if(Directory.Exists(assemblyDirectory)) {
                    Directory.Delete(assemblyDirectory, true);
                }
                Directory.CreateDirectory(assemblyDirectory);

                FileSystemUtils.CopyDirectory(Path.GetDirectoryName(project.Path), assemblyDirectory, blacklistedFileExtension, BlacklistedDirectory);

                AssemblyDefinition assemblyDefinition = AssemblyDefinition.LoadFromInclude(assemblyName, project.References);
                assemblyDefinition.AssemblyPath = assemblyPath;
                assemblyDefinition.Save();

                assemblyDefinitions[i] = assemblyDefinition;
            }

            return assemblyDefinitions;
        }
    }
}
