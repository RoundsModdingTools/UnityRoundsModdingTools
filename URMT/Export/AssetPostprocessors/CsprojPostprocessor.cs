using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using URMT.Core.Utils;
using URMT.Export.ScriptableObjects;

namespace URMT.Export.AssetPostprocessors {
    internal class CsprojPostprocessor : AssetPostprocessor {
        private static List<ModInfo> modInfos = null;

        public static string OnGeneratedCSProject(string path, string content) {
            if(!Application.platform.ToString().Contains("Windows")) {
                LoggerUtils.LogWarning($"Powershell scripts are only supported on Windows. Platform: {Application.platform}");
                return content;
            }

            string[] lines = content.Split('\n');
            List<string> newLines = lines.ToList();

            if(modInfos == null) {
                modInfos = new List<ModInfo>();
                string[] modInfoGuids = AssetDatabase.FindAssets("t:ModInfo");
                foreach(string guid in modInfoGuids) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    ModInfo modInfo = AssetDatabase.LoadAssetAtPath<ModInfo>(assetPath);

                    if (modInfo.ModAssemblyDefinition == null) continue;
                    modInfos.Add(modInfo);
                }
            }

            ModInfo mod = modInfos.FirstOrDefault(m => path.Contains(m.ModAssemblyDefinition.Name));
            if(mod == null) return content;

            for(int i = 0; i < lines.Length; i++) {
                if(lines[i].Contains("<PropertyGroup>")) {
                    newLines.Insert(i + 1, $"    <PreBuildEvent>powershell -ExecutionPolicy Bypass -File \"{mod.BeforeBuildPowerShell.Path}\"</PreBuildEvent>");
                    newLines.Insert(i + 1, $"    <PostBuildEvent>powershell -ExecutionPolicy Bypass -File \"{mod.AfterBuildPowerShell.Path}\"</PostBuildEvent>");
                    
                    mod.BeforeBuildPowerShell.GenerateScript();
                    mod.AfterBuildPowerShell.GenerateScript();
                    break;
                }
            }

            return string.Join("\n", newLines);
        }
        //public static string OnGeneratedSlnSolution(string path, string content) {
        //    return content;
        //}

    }
}
