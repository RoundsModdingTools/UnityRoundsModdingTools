using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityRoundsModdingTools.Editor.Utils;

namespace UnityRoundsModdingTools.Editor.ScriptableObjects {
    [CreateAssetMenu(fileName = "ModInfo", menuName = "Unity Rounds Modding Tools/Mod Info", order = 0)]
    public class ModInfo : ScriptableObject {
        public string ModName;
        public string Version = "1.0.0";
        public string WebsiteURL;
        public string Description;
        public string[] dependencies;

        public AssemblyDefinition ModAssemblyDefinition {
            get {
                string assetPath = AssetDatabase.GetAssetPath(this);
                string parentDirectoryPath = new FileInfo(assetPath).Directory.FullName; 

                string[] assemblyDefinitionPaths = Directory.GetFiles(parentDirectoryPath, "*.asmdef");
                if(assemblyDefinitionPaths.Count() == 0)
                    return null;
                return AssemblyDefinition.Load(assemblyDefinitionPaths[0]);
            }
        }
    }
}
