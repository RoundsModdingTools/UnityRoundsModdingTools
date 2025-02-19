using BepInEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace UnityRoundsModdingTools.Editor.Utils {
    public static class ProjectUtils {
        public static List<string> ConvertToUnityProject(string directoryPath, bool refresh = true) {
            List<string> csprojFiles = Directory.GetFiles(directoryPath, "*.csproj", SearchOption.AllDirectories).ToList();
            List<string> asmdefFiles = Directory.GetFiles(directoryPath, "*.asmdef", SearchOption.AllDirectories).ToList();

            List<AssemblyDefinition> assemblyDefinitions = new List<AssemblyDefinition>();

            if(csprojFiles.Count > 0) {
                foreach(string csprojFile in csprojFiles) {
                    string csprojFilePath = Path.Combine(Path.GetDirectoryName(directoryPath), csprojFile);

                    string rootNamespace = GetRootNamespace(csprojFilePath);
                    string assemblyName = GetAssemblyName(csprojFilePath) ?? rootNamespace;

                    if(assemblyName.IsNullOrWhiteSpace()) {
                        EditorUtility.DisplayDialog("Error", "Failed to get assembly name from the .csproj file.", "OK");
                        return null;
                    }

                    List<string> Includes = GetReferencesFromCsproj(csprojFilePath);
                    if(AssemblyDefinition.All.Any(x => x.Name == "ILGenerator")) Includes.Add("ILGenerator");

                    // We are removing System and Microsoft references because 'AssemblyDefinition' automatically includes them.
                    Includes.RemoveAll(x => x.StartsWith("System") || x.StartsWith("Microsoft."));

                    string modFolderCopyPath = Path.Combine(FileSystemManager.ModsFolderPath, assemblyName);
                    if(Directory.Exists(modFolderCopyPath)) Directory.Delete(modFolderCopyPath, true);
                    Directory.CreateDirectory(modFolderCopyPath);
                    FileSystemManager.CopyDirectory(Path.GetDirectoryName(csprojFilePath), modFolderCopyPath);

                    var assemblyDefinition = AssemblyDefinition.LoadFromInclude(assemblyName, Includes);
                    assemblyDefinition.AssemblyPath = Path.Combine(modFolderCopyPath, assemblyName + ".asmdef");
                    assemblyDefinition.Save();

                    assemblyDefinitions.Add(assemblyDefinition);
                }
            } else if(asmdefFiles.Count > 0) {
                UnityEngine.Debug.LogWarning("No .csproj files found in the directory, skipping conversion.");

                FileSystemManager.CopyDirectory(directoryPath, Path.Combine(FileSystemManager.ModsFolderPath, Path.GetFileName(directoryPath)));

                foreach(var assemblyDefinitionFile in asmdefFiles) {
                    string assemblyDefinitionFilePath = Path.Combine(Path.GetDirectoryName(directoryPath), assemblyDefinitionFile);
                    var assemblyDefinitionClass = AssemblyDefinition.Load(assemblyDefinitionFilePath);

                    assemblyDefinitions.Add(assemblyDefinitionClass);
                }
            } else {
                UnityEngine.Debug.LogError($"No .csproj or .asmdef files found in the provided directory '{directoryPath}'. Please make sure the directory contains at least one .csproj or .asmdef file to proceed.");
                return null;
            }

            return assemblyDefinitions.Select(assemblyDefinition => assemblyDefinition.Name).ToList();
        }

        private static string GetRootNamespace(string csprojFilePath) {
            string xmlData = File.ReadAllText(csprojFilePath);
            string pattern = @"<RootNamespace>(.*?)<\/RootNamespace>";
            Match match = Regex.Match(xmlData, pattern);

            if(match.Groups[1].Value == "") return Path.GetFileNameWithoutExtension(csprojFilePath);
            return match.Groups[1].Value;
        }

        private static string GetAssemblyName(string csprojFilePath) {
            string xmlData = File.ReadAllText(csprojFilePath);
            string pattern = @"<AssemblyName>(.*?)<\/AssemblyName>";
            Match match = Regex.Match(xmlData, pattern);

            if(match.Groups[1].Value == "") return Path.GetFileNameWithoutExtension(csprojFilePath);
            return match.Groups[1].Value;
        }

        private static List<string> GetReferencesFromCsproj(string csprojFilePath) {
            List<string> references = new List<string>();
            string xmlData = File.ReadAllText(csprojFilePath);
            string pattern = @"<(PackageReference|Reference|ProjectReference) Include=""([^"",]+).*""\s*\/?>";
            MatchCollection matches = Regex.Matches(xmlData, pattern);

            foreach(Match match in matches) {
                string reference = match.Groups[2].Value.Split('/').Last().Replace(".csproj", "");
                references.Add(reference);
            }

            return references;
        }
    }
}