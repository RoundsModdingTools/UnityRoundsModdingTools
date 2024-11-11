using Assets.Plugins.UnityRoundsModdingTools.Editor.Windows;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.Providers {
    internal static class ToolSettingsProvider {
        [SettingsProvider]
        public static SettingsProvider CreateToolSettingsProvider() {
            var provider = new UnityEditor.SettingsProvider("Project/Unity Rounds Modding Tools/Settings", SettingsScope.Project) {
                label = "Settings",
                guiHandler = (searchContext) => {
                    SettingsGUI.Instance.RenderSettings();
                }
            };

            return provider;
        }
    }
}
