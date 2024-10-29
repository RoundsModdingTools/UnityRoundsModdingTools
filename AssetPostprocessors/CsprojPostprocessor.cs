using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityRoundsModdingTools.ScriptableObjects;

namespace UnityRoundsModdingTools.AssetPostprocessors {
    internal class CsprojPostprocessor : AssetPostprocessor {
        private static List<ProjectMapping> ModBundleMap => ProjectMappings.Instance.projectMappings;


        private static List<string> Folders => ProjectMappings.Instance.GetFolderNames();
        private static List<FolderMapping> FolderMap => ProjectMappings.Instance.folderMappings;

        public static string OnGeneratedCSProject(string path, string content) {
            if (File.Exists("Assets/Editor/CsprojPostprocessor.cs")) return content;

            foreach(var mod in ModBundleMap.Select(projectMapping => projectMapping.ModName)) {
                if(path.EndsWith($"{mod}.csproj")) {
                    string newContent = "";
                    bool Added = false;
                    var lines = content.Split('\n');
                    foreach(var line in lines) {
                        if(!Added && line.Contains("<Compile Include=")) {
                            int index = ModBundleMap.FindIndex(folder => folder.ModName == mod);
                            newContent += $"     <EmbeddedResource Include=\"Assets\\AssetBundles\\{ModBundleMap[index].AssetBundleName}\" />\n";
                            Added = true;
                        }
                        
                        newContent += line + "\n";
                    }
                    return newContent;
                }
            }
            return content;
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
