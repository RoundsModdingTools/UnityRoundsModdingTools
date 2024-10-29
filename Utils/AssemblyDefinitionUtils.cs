using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityRoundsModdingTools.ScriptableObjects;

namespace UnityRoundsModdingTools.Utils {
    public static class AssemblyDefinitionUtils {
        private static Settings Settings => Settings.Instance;

        public static void CreateAssemblyDefinition(string assemblyName, string path, List<string> references, List<string> precompiledReferences) {
            AssemblyDefinitionClass assemblyDefinitionClass = new AssemblyDefinitionClass();
            assemblyDefinitionClass.name = assemblyName;
            assemblyDefinitionClass.references = references;
            assemblyDefinitionClass.optionalUnityReferences = new List<string>();
            assemblyDefinitionClass.includePlatforms = new List<string>();
            assemblyDefinitionClass.excludePlatforms = new List<string>();
            assemblyDefinitionClass.allowUnsafeCode = true;
            assemblyDefinitionClass.overrideReferences = true;
            assemblyDefinitionClass.precompiledReferences = precompiledReferences;
            assemblyDefinitionClass.autoReferenced = true;
            assemblyDefinitionClass.defineConstraints = new List<string>();

            string SerializeAssemblyDefinitionClass = JsonConvert.SerializeObject(assemblyDefinitionClass, Formatting.Indented);

            File.WriteAllText(Path.Combine(path, $"{assemblyName}.asmdef"), SerializeAssemblyDefinitionClass);
        }

        public static void CreateAssemblyDefinition(string assemblyName, string path, List<string> Includes) {
            (List<string> references, List<string> precompiledReferences) = ConvertIncludesToReferences(Includes);
            CreateAssemblyDefinition(assemblyName, path, references, precompiledReferences);
        }

        public static (List<string>, List<string>) ConvertIncludesToReferences(List<string> Includes) {
            List<string> references = new List<string>();
            List<string> precompiledReferences = new List<string>();

            foreach(string Include in Includes) {
                if((File.Exists(Path.Combine(FileSystemManager.DllsFolderPath, $"{Include}.dll")) &&
                    !Settings.WhitelistedUnityReferences.Contains(Include)) ||
                    File.Exists(Path.Combine(FileSystemManager.BepinexAndHarmonyFolderPath, $"{Include}.dll")))

                    precompiledReferences.Add($"{Include}.dll");
                else if(!Include.StartsWith("Unity") ||
                    Settings.WhitelistedUnityReferences.Contains(Include))

                    references.Add(Include);
            }

            return (references, precompiledReferences);
        }

        public static string[] GetAllAssemblyDefinitionPath() {
            return Array.FindAll(AssetDatabase.GetAllAssetPaths(), path => path.Contains(".asmdef") && !path.StartsWith("Packages"));
        }

        public static AssemblyDefinitionClass ParseAssemblyDefinitionFie(string assemblyDefinitionFilePath) {
            string assemblyDefinitionFileContent = File.ReadAllText(assemblyDefinitionFilePath);
            return JsonConvert.DeserializeObject<AssemblyDefinitionClass>(assemblyDefinitionFileContent);
        }
    }
    public class AssemblyDefinitionClass {
        public string name { get; set; }
        public List<string> references { get; set; }
        public List<string> optionalUnityReferences { get; set; }
        public List<string> includePlatforms { get; set; }
        public List<string> excludePlatforms { get; set; }
        public bool allowUnsafeCode { get; set; }
        public bool overrideReferences { get; set; }
        public List<string> precompiledReferences { get; set; }
        public bool autoReferenced { get; set; }
        public List<string> defineConstraints { get; set; }
    }
}