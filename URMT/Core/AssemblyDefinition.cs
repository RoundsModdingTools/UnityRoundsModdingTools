using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace URMT.Core {
    public class AssemblyDefinition {
        public static readonly string[] DefaultReferences = new string[] {
            "UnityEngine",
            "UnityEditor",
            "System",
            "Microsoft.CSharp",
        };

        private AssemblyDefinitionAsset _asset;
        private static AssemblyDefinition[] _all;

        private static string[] _dlls;


        [JsonIgnore]
        public Assembly Assembly {
            get {
                try {
                    return Assembly.Load(Name);
                } catch(Exception e) {
                    Debug.LogError($"Failed to load assembly {Name}: {e.Message}");
                }
                return null;
            }
        }

        [JsonIgnore]
        private static string[] Dlls {
            get {
                if(_dlls == null) {
                    _dlls = Directory.GetFiles(Application.dataPath, "*.dll", SearchOption.AllDirectories)
                        .Select(Path.GetFileNameWithoutExtension)
                        .ToArray();
                }
                return _dlls;
            }
        }

        [JsonIgnore]
        public AssemblyDefinitionAsset Asset {
            get {
                if(_asset == null) {
                    _asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(AssemblyPath);
                }

                return _asset;
            }
        }

        [JsonIgnore]
        public static AssemblyDefinition[] All {
            get {
                // This saves a lot of performance by not loading all the assembly definitions every time.
                if(_all == null) {
                    string[] assemblyDefinitionAssets = AssetDatabase.FindAssets($"t:{typeof(AssemblyDefinitionAsset).Name}");
                    string[] assemblyDefinitionPaths = assemblyDefinitionAssets.Select(AssetDatabase.GUIDToAssetPath).ToArray();
                    _all = assemblyDefinitionPaths.Select(Load).ToArray();
                }

                return _all;
            }
        }

        [JsonIgnore] public string AssemblyPath { get; set; }

        [JsonProperty("name")] public string Name { get; private set; }

        [JsonProperty("allowUnsafeCode")] public bool AllowUnsafeCode = true;
        [JsonProperty("overrideReferences")] public bool OverrideReferences = true;
        [JsonProperty("autoReferenced")] public bool AutoReferenced = true;

        [JsonProperty("defineConstraints")] public List<string> DefineConstraints = new List<string>();

        [JsonProperty("references")] public List<string> References = new List<string>();
        [JsonProperty("optionalUnityReferences")] public List<string> OptionalUnityReferences = new List<string>();
        [JsonProperty("precompiledReferences")] public List<string> PrecompiledReferences = new List<string>();

        [JsonProperty("includePlatforms")] public List<string> IncludePlatforms = new List<string>();
        [JsonProperty("excludePlatforms")] public List<string> ExcludePlatforms = new List<string>();

        private AssemblyDefinition() { }
        public AssemblyDefinition(string assemblyName) {
            Name = assemblyName;
        }
        public AssemblyDefinition(string assemblyName, string[] Includes) {
            Name = assemblyName;
            (References, PrecompiledReferences) = ConvertIncludesToReferences(Includes);
        }

        public static AssemblyDefinition LoadFromInclude(string assemblyName, string[] Includes) {
            (List<string> references, List<string> precompiledReferences) = ConvertIncludesToReferences(Includes);

            AssemblyDefinition assemblyDefinitionClass = new AssemblyDefinition(assemblyName);
            assemblyDefinitionClass.References = references;
            assemblyDefinitionClass.PrecompiledReferences = precompiledReferences;

            return assemblyDefinitionClass;
        }
        public static AssemblyDefinition LoadFromAssemblyDefinitionAsset(AssemblyDefinitionAsset assemblyDefinition) {
            AssemblyDefinition assemblyDefinitionClass = JsonConvert.DeserializeObject<AssemblyDefinition>(assemblyDefinition.text);
            assemblyDefinitionClass.AssemblyPath = AssetDatabase.GetAssetPath(assemblyDefinition);

            return assemblyDefinitionClass;
        }
        public static AssemblyDefinition Load(string path) {
            AssemblyDefinition assemblyDefinitionClass = JsonConvert.DeserializeObject<AssemblyDefinition>(File.ReadAllText(path));
            assemblyDefinitionClass.AssemblyPath = path;

            return assemblyDefinitionClass;
        }

        public static AssemblyDefinition LoadFromName(string assemblyName) {
            AssemblyDefinition assemblyDefinition = All.FirstOrDefault(x => x.Name == assemblyName);
            return assemblyDefinition;
        }

        public void Save() {
            if(AssemblyPath == null) throw new IOException("Path is not set for AssemblyDefinition");

            File.WriteAllText(AssemblyPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        private static (List<string>, List<string>) ConvertIncludesToReferences(string[] Includes) {
            List<string> references = new List<string>();
            List<string> precompiledReferences = new List<string>();

            foreach(string Include in Includes) {
                bool isPrecompiled = Dlls.Contains(Include);
                bool isDefault = DefaultReferences.Any(x => Include.StartsWith(x));

                if(isPrecompiled && !isDefault) {
                    precompiledReferences.Add($"{Include}.dll");
                } else if(!isDefault) {
                    references.Add(Include);
                }
            }

            return (references, precompiledReferences);
        }
    }
}