﻿using BepInEx;
using Boo.Lang.Runtime.DynamicDispatching;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityRoundsModdingTools.Editor.CustomInspector;
using UnityRoundsModdingTools.Editor.Utils;

namespace UnityRoundsModdingTools.Editor.ScriptableObjects {
    [CreateAssetMenu(fileName = "ModInfo", menuName = "Unity Rounds Modding Tools/Mod Info", order = 0)]
    public class ModInfo : ScriptableObject {
        private FileSystemWatcher FileSystemWatcher = new FileSystemWatcher {
            Path = Path.Combine("obj"),
            Filter = "*.dll",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };
        private List<string> WatchedFiles = new List<string>();

        public string ModName;
        public string Version = "1.0.0";
        public string WebsiteURL;
        public string Description;
        public List<string> Dependencies = new List<string>();

        public List<string> DllDependencies = new List<string>();
        public List<string> AssemblyDefinitionDependencies = new List<string>();

        public void OnEnable() {
            FileSystemWatcher.Changed += OnFileChanged;
            
            WatchedFiles.Clear();
            if(ModAssemblyDefinition != null) {
                WatchedFiles.Add(Path.GetFullPath(GetDLLObjPath(ModAssemblyDefinition.Assembly.Location)));
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e) {
            if(!Settings.Instance.AutoPublish || !WatchedFiles.Contains(e.FullPath)) return;

            UnityEngine.Debug.Log($"Detected file change, republishing mod '{ModName}'");
            MainThreadAction.Invoke(PublishMod);
        }

        public static void PublishAll() {
            string[] modInfoGuids = AssetDatabase.FindAssets("t:ModInfo");
            List<ModInfo> modInfos = new List<ModInfo>();

            foreach(string guid in modInfoGuids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ModInfo modInfo = AssetDatabase.LoadAssetAtPath<ModInfo>(assetPath);
                modInfos.Add(modInfo);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach(ModInfo modInfo in modInfos) {
                modInfo.PublishMod();
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Published {modInfos.Count} mods in {stopwatch.ElapsedMilliseconds}ms");
        }

        public AssemblyDefinition ModAssemblyDefinition {
            get {
                string assetPath = AssetDatabase.GetAssetPath(this);
                string parentDirectoryPath = new FileInfo(assetPath).Directory.FullName;

                string[] assemblyDefinitionPaths = Directory.GetFiles(parentDirectoryPath, "*.asmdef");
                return assemblyDefinitionPaths.Length > 0
                    ? AssemblyDefinition.Load(assemblyDefinitionPaths[0])
                    : null;
            }
        }

        public void PublishMod() {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string modDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
            string publishPath = Path.Combine(Settings.Instance.PublishPath, ModName);

            string readmePath = Path.Combine(modDirectory, "README.md");
            string iconPath = Path.Combine(modDirectory, "icon.png");
            string changelogPath = Path.Combine(modDirectory, "CHANGELOG.md");

            if(Directory.Exists(publishPath)) Directory.Delete(publishPath, true);
            if(File.Exists($"{publishPath}.zip")) File.Delete($"{publishPath}.zip");
            Directory.CreateDirectory(publishPath);


            Manifest manifest = new Manifest {
                ModName = ModName,
                Version = Version,
                WebsiteURL = WebsiteURL,
                Description = Description,
                Dependencies = Dependencies,
            };

            File.WriteAllText(Path.Combine(publishPath, "manifest.json"), JsonConvert.SerializeObject(manifest, Formatting.Indented));

            Directory.CreateDirectory(Path.Combine(publishPath, "plugins"));
            if(ModAssemblyDefinition != null) {
                string DllObjPath = GetDLLObjPath(ModAssemblyDefinition.Assembly.Location);
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

            if(AssemblyDefinitionDependencies != null && AssemblyDefinitionDependencies.Count > 0) {
                if(!Directory.Exists(Path.Combine(publishPath, "dependencies")) && ModAssemblyDefinition != null) {
                    Directory.CreateDirectory(Path.Combine(publishPath, "dependencies"));
                }
                foreach(string assemblyDefinitionDependency in AssemblyDefinitionDependencies) {
                    AssemblyDefinition assemblyDefinition = AssemblyDefinition.LoadFromName(assemblyDefinitionDependency);
                    string dllPath = GetDLLObjPath(assemblyDefinition.Assembly.Location);

                    File.Copy(dllPath, Path.Combine(publishPath, ModAssemblyDefinition != null ? "dependencies" : "plugins", $"{assemblyDefinition.Name}.dll"));
                }
            }

            ZipFile.CreateFromDirectory(publishPath, $"{publishPath}.zip");

            if(!Settings.Instance.PublishFolderCopyTo.IsNullOrWhiteSpace() && Directory.Exists(Settings.Instance.PublishFolderCopyTo)) {
                string copyToPath = Path.Combine(Settings.Instance.PublishFolderCopyTo, $"Unknown-{ModName}");
                if(Directory.Exists(copyToPath)) Directory.Delete(copyToPath, true);

                // Copy the folder to the specified path
                FileSystemManager.CopyDirectory(publishPath, copyToPath, new string[] { }, new string[] { });
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Published mod {ModName} in {stopwatch.ElapsedMilliseconds}ms");
        }

        public string GetDLLObjPath(string dllPath) {
            string debugDllPath = Path.Combine("obj", "Debug", Path.GetFileName(dllPath));
            string releaseDllPath = Path.Combine("obj", "Release", Path.GetFileName(dllPath));

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

            return dllPath;
        }
    }
}
