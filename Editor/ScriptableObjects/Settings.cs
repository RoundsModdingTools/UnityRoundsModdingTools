using System.Collections.Generic;
using UnityEngine;

namespace UnityRoundsModdingTools.Editor.ScriptableObjects {
    [CreateAssetMenu(fileName = "Settings", menuName = "Unity Rounds Modding Tools/Settings", order = 0)]
    internal class Settings : ScriptableSingleton<Settings> {
        [Header("Settings for Modding Tools")]
        public string DllsFolderPath = @"Scripts/dlls";
        public string ModsFolderPath = @"Scripts/Mods";
        public string BepinexAndHarmonyFolderPath = "Scripts/Mods/Bepinex and Harmony";
        public string TempPath = @"C:/Temp";

        [Header("Settings for Coverting Project to Unity Project")]
        public List<string> BlacklistedDirectory = new List<string>() { "obj", "bin", ".git", ".vs", "Assemblies" };
        public List<string> BlacklistedFileExtension = new List<string> { "csproj", "sln", "dll" };
        public List<string> WhitelistedUnityReferences = new List<string>() { "Unity.TextMeshPro", "Unity.TextMeshPro.Editor", "Unity.TextMeshPro.Editor.Tests", "Unity.TextMeshPro.Tests" };
    }
}
