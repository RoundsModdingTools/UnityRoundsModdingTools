using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityRoundsModdingTools.Editor.ScriptableObjects {
    [CreateAssetMenu(fileName = "ProjectMapping", menuName = "Unity Rounds Modding Tools/Project Mapping", order = 0)]
    public class ProjectMappings : ScriptableSingleton<ProjectMappings> {
        public List<ProjectMapping> projectMappings = new List<ProjectMapping>();

        public List<FolderMapping> folderMappings = new List<FolderMapping>() {
            new FolderMapping("CardChoiceSpawnUniqueCardPatch", "Libraries"),
            new FolderMapping("CardThemeLib", "Libraries"),
            new FolderMapping("ClassesManagerReborn", "Libraries"),
            new FolderMapping("ItemShops", "Libraries"),
            new FolderMapping("ModdingUtils", "Libraries"),
            new FolderMapping("ModsPlus", "Libraries"),
            new FolderMapping("PickNCards", "Libraries"),
            new FolderMapping("RarityLib", "Libraries"),
            new FolderMapping("RoundsWithFriends", "Libraries"),
            new FolderMapping("UnboundLib", "Libraries"),
            new FolderMapping("WillsWackyManagers", "Libraries"),
            new FolderMapping("ILGenerator", "Libraries"),
            new FolderMapping("UnityRoundsModdingTools.Editor", "UnityRoundsModdingTools"),
        };

        public List<string> GetFolderNames() {
            List<string> folderNames = new List<string>();

            foreach(FolderMapping folder in folderMappings) {
                if(!folderNames.Contains(folder.FolderName)) folderNames.Add(folder.FolderName);
            }

            return folderNames;
        }
    }

    [Serializable]
    public struct ProjectMapping {
        public string ModName;
        public string AssetBundleName;
        public ProjectMapping(string modName, string assetBundleName) {
            ModName = modName;
            AssetBundleName = assetBundleName;
        }
    }

    [Serializable]
    public struct FolderMapping {
        public string AssemblyName;
        public string FolderName;

        public FolderMapping(string assemblyName, string folderName) {
            AssemblyName = assemblyName;
            FolderName = folderName;
        }
    }
}
