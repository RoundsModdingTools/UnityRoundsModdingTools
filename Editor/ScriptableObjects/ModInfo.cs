using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityRoundsModdingTools.Editor.CustomInspector;
using UnityRoundsModdingTools.Editor.Utils;

namespace UnityRoundsModdingTools.Editor.ScriptableObjects {
    [CreateAssetMenu(fileName = "ModInfo", menuName = "Unity Rounds Modding Tools/Mod Info", order = 0)]
    public class ModInfo : ScriptableObject {
        public string ModName;
        public string Version = "1.0.0";
        public string WebsiteURL;
        public string Description;
        public List<string> Dependencies = new List<string>();
        public List<string> DllDependencies = new List<string>();

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

        public void PublishMod() {
            string modDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
            string publishPath = Path.Combine(Settings.Instance.PublishPath, ModName);

            string readmePath = Path.Combine(modDirectory, "README.md");
            string iconPath = Path.Combine(modDirectory, "icon.png");
            string changelogPath = Path.Combine(modDirectory, "CHANGELOG.md");

            if(Directory.Exists(publishPath)) Directory.Delete(publishPath, true);
            if(File.Exists($"{publishPath}.zip")) File.Delete($"{publishPath}.zip");
            Directory.CreateDirectory(publishPath);

            string DllObjPath = GetDLLObjPath(ModAssemblyDefinition.Assembly.Location);

            Manifest manifest = new Manifest {
                ModName = ModName,
                Version = Version,
                WebsiteURL = WebsiteURL,
                Description = Description,
                Dependencies = Dependencies,
            };

            File.WriteAllText(Path.Combine(publishPath, "manifest.json"), JsonConvert.SerializeObject(manifest, Formatting.Indented));

            if(DllObjPath != null) {
                Directory.CreateDirectory(Path.Combine(publishPath, "plugins"));
                File.Copy(DllObjPath, Path.Combine(publishPath, "plugins", Path.GetFileName(DllObjPath)));
            }
            if(File.Exists(readmePath)) File.Copy(readmePath, Path.Combine(publishPath, "README.md"));
            if(File.Exists(iconPath)) File.Copy(iconPath, Path.Combine(publishPath, "icon.png"));
            if(File.Exists(changelogPath)) File.Copy(changelogPath, Path.Combine(publishPath, "CHANGELOG.md"));

            if(DllDependencies != null && DllDependencies.Count > 0) {
                Directory.CreateDirectory(Path.Combine(publishPath, "dependencies"));
                foreach(string dllDependency in DllDependencies) {
                    string dllPath = CompilationPipeline.GetPrecompiledAssemblyPathFromAssemblyName(dllDependency);
                    File.Copy(dllPath, Path.Combine(publishPath, "dependencies", $"{dllDependency}"));
                }
            }


            ZipFile.CreateFromDirectory(publishPath, $"{publishPath}.zip");

            if(!Settings.Instance.PublishFolderCopyTo.IsNullOrWhiteSpace() && Directory.Exists(Settings.Instance.PublishFolderCopyTo)) {
                string copyToPath = Path.Combine(Settings.Instance.PublishFolderCopyTo, $"Unknown-{ModName}");
                if(Directory.Exists(copyToPath)) Directory.Delete(copyToPath, true);

                // Copy the folder to the specified path
                FileSystemManager.CopyDirectory(publishPath, copyToPath, new string[] { }, new string[] { });
            }
        }

        public string GetDLLObjPath(string dllPath) {
            string debugDllPath = $"obj/Debug/{Path.GetFileName(dllPath)}";
            string releaseDllPath = $"obj/Release/{Path.GetFileName(dllPath)}";

            bool debugExists = File.Exists(debugDllPath);
            bool releaseExists = File.Exists(releaseDllPath);

            if(debugExists && releaseExists) {
                DateTime debugLastModified = File.GetLastWriteTime(debugDllPath);
                DateTime releaseLastModified = File.GetLastWriteTime(releaseDllPath);

                if(debugLastModified > releaseLastModified) {
                    return debugDllPath;
                } else {
                    return releaseDllPath;
                }
            }

            if(debugExists) return debugDllPath;
            if(releaseExists) return releaseDllPath;

            return null;
        }
    }
}
