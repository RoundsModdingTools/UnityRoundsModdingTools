using UnityEditor;
using UnityRoundsModdingTools.Editor.Windows;

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
