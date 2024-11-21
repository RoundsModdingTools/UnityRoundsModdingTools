using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityRoundsModdingTools.Editor.ScriptableObjects {
    [CreateAssetMenu(fileName = "Settings", menuName = "Unity Rounds Modding Tools/Settings", order = 0)]
    internal class Settings : ScriptableSingleton<Settings> {
        [Header("Settings for Modding Tools")]
        public string DllsFolderPath = "Scripts\\dlls";
        public string ModsFolderPath = "Scripts\\Mods";
        public string BepinexFolderPath = "Scripts\\Mods\\Bepinex and Harmony";
        public string TempPath = $"{Path.GetTempPath()}UnityRoundsModdingTools";
        public string PublishPath = "Publish";

        [Header("Settings for Creating Mod")]
        public string TemplatePath = "https://github.com/RoundsModdingTools/UnityRoundsModTemplate";
        public string TemplateOutputPath = "Assets\\Mods";

        [Header("Settings for Coverting Project to Unity Project")]
        [HideInInspector] public List<string> BlacklistedDirectory = new List<string>() { "obj", "bin", ".vs", "Assemblies" };
        [HideInInspector] public List<string> BlacklistedFileExtension = new List<string> { "csproj", "sln", "dll" };
        [HideInInspector] public List<string> WhitelistedUnityReferences = new List<string>() { "Unity.TextMeshPro", "Unity.TextMeshPro.Editor", "Unity.TextMeshPro.Editor.Tests", "Unity.TextMeshPro.Tests" };
    }
}
