using UnityEditor;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.MenuItems {
    public class PublishAllMods {
        [MenuItem("Assets/Publish All Mods")]
        static void PublishAll() {
            string[] modInfoGuids = AssetDatabase.FindAssets("t:ModInfo");
            foreach(string guid in modInfoGuids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ModInfo modInfo = AssetDatabase.LoadAssetAtPath<ModInfo>(assetPath);
                modInfo.PublishMod();
            }
        }
    }
}
