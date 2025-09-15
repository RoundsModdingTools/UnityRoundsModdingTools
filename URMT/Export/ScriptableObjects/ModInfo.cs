using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using URMT.Core;
using URMT.Core.Utils;
using URMT.Export.Entities;
using URMT.Networking;

namespace URMT.Export.ScriptableObjects {
    [CreateAssetMenu(fileName = "ModInfo", menuName = "URMT/Mod Info", order = 0)]
    public class ModInfo : ScriptableObject {
        public string ModName;
        public string Author;
        public string Version = "1.0.0";
        public string WebsiteURL;
        public string Description;

        public string BeforeBuildCommand;
        public string AfterBuildCommand;

        public PowerShellCodeGenerator BeforeBuildPowerShell;
        public PowerShellCodeGenerator AfterBuildPowerShell;

        public List<string> Dependencies = new List<string>();

        public List<string> DllDependencies = new List<string>();
        public List<string> AssemblyDefinitionDependencies = new List<string>();

        public Texture2D Icon;

        public bool HasReadme => File.Exists(Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)), "README.md"));
        public bool HasIcon => Icon != null && AssetDatabase.GetAssetPath(Icon).EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        public bool HasChangelog => File.Exists(Path.Combine(Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)), "CHANGELOG.md"));

        public bool IncludeInAllExports = true;

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

        public void OnEnable() {
            CreatePowerShellScript();
            GenerateScripts();
        }

        public void CreatePowerShellScript() {
            if (ModAssemblyDefinition == null) return;

            string resourcePath = Path.Combine(Application.dataPath, "Resources", "UnityRoundsModdingTools", "ModPowerShellScripts", ModAssemblyDefinition.Name);
            if(!Directory.Exists(resourcePath)) Directory.CreateDirectory(resourcePath);

            string beforeBuildPath = Path.Combine(resourcePath, "BeforeBuild.ps1");
            string afterBuildPath = Path.Combine(resourcePath, "AfterBuild.ps1");

            BeforeBuildPowerShell = new PowerShellCodeGenerator(beforeBuildPath);
            AfterBuildPowerShell = new PowerShellCodeGenerator(afterBuildPath);

            BeforeBuildPowerShell.AddLine($"$AssemblyName = \"{ModAssemblyDefinition.Name}\"");
            BeforeBuildPowerShell.AddLine($"$AssemblyPath = \"{ModAssemblyDefinition.AssemblyPath}\"");
            BeforeBuildPowerShell.AddLine($"$MessageServerIP = \"{NetworkingModuleSettings.Instance.Port}\"");

            AfterBuildPowerShell.AddLine($"$AssemblyName = \"{ModAssemblyDefinition.Name}\"");
            AfterBuildPowerShell.AddLine($"$AssemblyPath = \"{ModAssemblyDefinition.AssemblyPath}\"");
            AfterBuildPowerShell.AddLine($"$MessageServerIP = \"{NetworkingModuleSettings.Instance.Port}\"");

            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("$client = New-Object System.Net.Sockets.TcpClient('127.0.0.1', $MessageServerIP);");
            builder.AppendLine("$stream = $client.GetStream();");
            builder.AppendLine("$writer = New-Object System.IO.StreamWriter($stream);");
            builder.AppendLine("$writer.AutoFlush = $true;");
            builder.AppendLine("$writer.WriteLine('\"ExportAssemblyAuto\", \"' + $AssemblyName + '\"');");
            builder.AppendLine("$client.Close();");

            AfterBuildPowerShell.AddLine(builder.ToString());
            
            BeforeBuildPowerShell.AddLine($"\n{BeforeBuildCommand}");
            AfterBuildPowerShell.AddLine(AfterBuildCommand);
        }

        public void GenerateScripts() {
            if(ModAssemblyDefinition == null) return;
            else if(!Application.platform.ToString().Contains("Windows")) {
                UnityEngine.Debug.LogWarning($"Powershell scripts are only supported on Windows. Platform: {Application.platform}");
                return;
            }

            BeforeBuildPowerShell.GenerateScript();
            AfterBuildPowerShell.GenerateScript();
        }

        public static void GenerateAllScripts() {
            if (!Application.platform.ToString().Contains("Windows")) {
                UnityEngine.Debug.LogWarning($"Powershell scripts are only supported on Windows. Platform: {Application.platform}");
                return;
            }

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
                modInfo.CreatePowerShellScript();
                modInfo.GenerateScripts();
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log($"Generated scripts for {modInfos.Count} mods in {stopwatch.ElapsedMilliseconds}ms");
        }

        public static void ExportAll() {
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
                if(!modInfo.IncludeInAllExports) return;

                modInfo.ExportMod();
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Exported {modInfos.Count} mods in {stopwatch.ElapsedMilliseconds}ms");
        }

        public static void ExportAssembly(string AssemblyName) {
            string[] modInfoGuids = AssetDatabase.FindAssets("t:ModInfo");

            ModInfo mod = null;
            foreach(string guid in modInfoGuids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ModInfo modInfo = AssetDatabase.LoadAssetAtPath<ModInfo>(assetPath);
                if (modInfo.ModAssemblyDefinition?.Name == AssemblyName) {
                    mod = modInfo;
                }
            }

            if (mod != null) {
                mod.ExportMod();
            }
        }

        public string ExportMod() {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string modDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
            string ExportPath = Path.Combine(ExportModuleSettings.Instance.ExportPath, ModName);

            string readmePath = Path.Combine(modDirectory, "README.md");
            string changelogPath = Path.Combine(modDirectory, "CHANGELOG.md");

            if(Directory.Exists(ExportPath)) Directory.Delete(ExportPath, true);
            if(File.Exists($"{ExportPath}.zip")) File.Delete($"{ExportPath}.zip");
            Directory.CreateDirectory(ExportPath);


            Manifest manifest = new Manifest {
                ModName = ModName,
                Version = Version,
                WebsiteURL = WebsiteURL,
                Description = Description,
                Dependencies = Dependencies.ToArray()
            };

            File.WriteAllText(Path.Combine(ExportPath, "manifest.json"), JsonConvert.SerializeObject(manifest, Formatting.Indented));

            Directory.CreateDirectory(Path.Combine(ExportPath, "plugins"));
            if(ModAssemblyDefinition != null) {
                string DllObjPath = GetDLLObjPath(ModAssemblyDefinition.Assembly.Location);
                File.Copy(DllObjPath, Path.Combine(ExportPath, "plugins", Path.GetFileName(DllObjPath)));
            }

            if(HasReadme) File.Copy(readmePath, Path.Combine(ExportPath, "README.md"));
            if(HasIcon) File.Copy(AssetDatabase.GetAssetPath(Icon), Path.Combine(ExportPath, "icon.png"));
            if(HasChangelog) File.Copy(changelogPath, Path.Combine(ExportPath, "CHANGELOG.md"));

            if(DllDependencies != null && DllDependencies.Count > 0) {
                Directory.CreateDirectory(Path.Combine(ExportPath, "dependencies"));
                foreach(string dllDependency in DllDependencies) {
                    string dllPath = CompilationPipeline.GetPrecompiledAssemblyPathFromAssemblyName(dllDependency);
                    File.Copy(dllPath, Path.Combine(ExportPath, "dependencies", $"{dllDependency}"));
                }
            }

            if(AssemblyDefinitionDependencies != null && AssemblyDefinitionDependencies.Count > 0) {
                if(!Directory.Exists(Path.Combine(ExportPath, "dependencies")) && ModAssemblyDefinition != null) {
                    Directory.CreateDirectory(Path.Combine(ExportPath, "dependencies"));
                }
                foreach(string assemblyDefinitionDependency in AssemblyDefinitionDependencies) {
                    AssemblyDefinition assemblyDefinition = AssemblyDefinition.LoadFromName(assemblyDefinitionDependency);
                    string dllPath = GetDLLObjPath(assemblyDefinition.Assembly.Location);

                    File.Copy(dllPath, Path.Combine(ExportPath, ModAssemblyDefinition != null ? "dependencies" : "plugins", $"{assemblyDefinition.Name}.dll"));
                }
            }

            ZipFile.CreateFromDirectory(ExportPath, $"{ExportPath}.zip");

            if(!string.IsNullOrWhiteSpace(ExportModuleSettings.Instance.ExportFolderCopyTo) && Directory.Exists(ExportModuleSettings.Instance.ExportFolderCopyTo)) {
                string copyToPath = Path.Combine(ExportModuleSettings.Instance.ExportFolderCopyTo, $"unknow-{ModName}");
                if(Directory.Exists(copyToPath)) Directory.Delete(copyToPath, true);

                // Copy the folder to the specified path
                FileSystemUtils.CopyDirectory(ExportPath, copyToPath);
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Exported mod {ModName} in {stopwatch.ElapsedMilliseconds}ms");

            return ExportPath;
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
