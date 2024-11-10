using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.Utils.Template {
    public class ModTemplateHandler : ITemplateHandler {
        public string modId;
        public string modName;
        public string modInitial;
        public string version;

        private string safeModName => modName.Replace(" ", "").Replace("-", "");

        public string[] bepInDependencies;
        public string modPath;

        public string HandleTemplate(string content) {
            Regex bepInDependenciesRegex = new Regex(@"(\s*){{BEPIN_DEPENDENCIES}}");
            string bepInDependenciesWhitespace = bepInDependenciesRegex.Match(content).Groups[1].Value;

            StringBuilder dependencies = new StringBuilder();
            foreach(var dependency in bepInDependencies) {
                dependencies.Append($"{bepInDependenciesWhitespace}[BepInDependency(\"{dependency}\", BepInDependency.DependencyFlags.HardDependency)]");
            }

            var template = content
                .Replace("{{MOD_ID}}", modId)
                .Replace("{{MOD_NAME}}", modName)
                .Replace("{{MOD_INITIAL}}", modInitial)
                .Replace("{{VERSION}}", version)
                .Replace("{{MOD_SAFE_NAME}}", safeModName);

            template = bepInDependenciesRegex.Replace(template, dependencies.ToString());

            return template;
        }

        public void AfterTemplateCompile() {
            Assembly assembly = Assembly.Load(safeModName);
            Type cardRegister = assembly.GetType($"{safeModName}.CardRegister");

            GameObject cardRegisterObj = new GameObject("CardRegister", cardRegister);

            if(!Directory.Exists($"{modPath}/Prefabs")) Directory.CreateDirectory($"{modPath}/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(cardRegisterObj, $"{modPath}/Prefabs/CardRegister.prefab");

            GameObject.DestroyImmediate(cardRegisterObj);

            AddAssetsToAssetBundle();

            EditorUtility.DisplayDialog("Mod Created", $"Mod '{modName}' has been successfully created at path: '{modPath.Replace(Path.DirectorySeparatorChar, '/')}'", "OK");
        }

        public void InitTemplate(params object[] args) {
            modId = (string)args[0];
            modName = (string)args[1];
            modInitial = (string)args[2];
            version = (string)args[3];

            bepInDependencies = (string[])args[4];
            modPath = (string)args[5];
        }

        private void AddAssetsToAssetBundle() {
            string[] assetGUIDS = AssetDatabase.FindAssets("t:GameObject", new string[] { modPath });
            foreach(var assetGUID in assetGUIDS) {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant($"{safeModName.ToLower()}_assets", "");
            }

            if(!ProjectMappings.Instance.projectMappings.Exists(mapping => mapping.ModName == safeModName))
                ProjectMappings.Instance.projectMappings.Add(new ProjectMapping(safeModName, $"{safeModName.ToLower()}_assets"));
        }
    }
}
