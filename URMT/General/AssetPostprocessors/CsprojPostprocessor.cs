using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using URMT.General.Entities;

namespace URMT.General.AssetPostprocessors {
    internal class CsprojPostprocessor : AssetPostprocessor {
        private static List<ModBundleMapping> ModBundleMap => GeneralModuleSettings.Instance.ModBundleMappings;

        private static List<FolderMapping> FolderMap => GeneralModuleSettings.Instance.FolderMappings;
        private static List<string> Folders => GeneralModuleSettings.Instance.FolderMappings
            .Select(mapping => mapping.FolderName)
            .Distinct()
            .ToList();


        public static string OnGeneratedCSProject(string path, string content) {
            if(File.Exists("Assets/Editor/CsprojPostprocessor.cs")) return content;

            StringBuilder newContent = new StringBuilder();
            var lines = content.Split('\n');

            bool Added = false;
            foreach(var line in lines) {
                newContent.AppendLine(line);
                
                if(!Added && line.Contains("<Compile Include=")) {
                    foreach(var mod in ModBundleMap.Where(projectMapping => path.EndsWith($"{projectMapping.ModName}.csproj"))) {
                        newContent.AppendLine($"     <EmbeddedResource Include=\"Assets\\AssetBundles\\{mod.AssetBundleName}\" />");
                        Added = true;
                    }
                }
            }

            return newContent.ToString();
        }

        public static string OnGeneratedSlnSolution(string path, string content) {
            if(File.Exists("Assets/Editor/CsprojPostprocessor.cs")) return content;

            string newContent = "";
            Dictionary<string, string> folderGuids = new Dictionary<string, string>();
            foreach(string folder in Folders) {
                folderGuids[folder] = Guid.NewGuid().ToString().ToUpper();
            }
            Dictionary<string, string> modGuids = new Dictionary<string, string>();
            var lines = content.Split('\n').Select(line => line.Trim('\r'));
            bool setup = false;
            foreach(string line in lines) {
                if(!setup) {
                    if(line == "Global") {
                        foreach(string folder in Folders) {
                            newContent += $"Project(\"{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}\") = \"{folder}\", \"{folder}\", \"{{{folderGuids[folder]}}}\"\nEndProject\n";
                        }
                        setup = true;
                    } else {
                        foreach(string mod in FolderMap.Select(folder => folder.AssemblyName)) {
                            if(line.Contains($"\"{mod}\"")) {
                                modGuids[mod] = line.Substring(69 + (mod.Length * 2), 36);
                            }
                        }
                    }
                } else {
                    if(line == "EndGlobal") {
                        newContent += "\tGlobalSection(NestedProjects) = preSolution\n";

                        foreach(string mod in FolderMap.Select(folder => folder.AssemblyName)) {
                            try {
                                int index = FolderMap.FindIndex(folder => folder.AssemblyName == mod);
                                newContent += $"\t\t{{{modGuids[mod]}}} = {{{folderGuids[FolderMap[index].FolderName]}}}\n";
                            } catch(Exception e) { UnityEngine.Debug.LogError($"{mod}\n{e}"); }
                        }

                        newContent += "\tEndGlobalSection\n";
                    }
                }
                newContent += line + "\n";
            }

            return newContent;
        }
    }
}
