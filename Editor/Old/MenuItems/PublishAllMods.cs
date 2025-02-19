using Boo.Lang;
using System.Diagnostics;
using UnityEditor;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.MenuItems {
    public class PublishAllMods {
        [MenuItem("Assets/Publish All Mods")]
        static void PublishAll() {
            ModInfo.PublishAll();
        }
    }
}
