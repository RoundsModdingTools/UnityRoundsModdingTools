using Boo.Lang;
using System.Diagnostics;
using UnityEditor;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.MenuItems {
    public class PublishAllMods {
        [MenuItem("Assets/Publish All Mods")]
        static void PublishAll() {
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
    }
}
