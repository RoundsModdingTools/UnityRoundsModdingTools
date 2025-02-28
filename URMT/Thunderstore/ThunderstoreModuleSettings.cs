using Microsoft.Win32;
using UnityEditor;
using UnityEngine;
using URMT.Core.ScriptableObjects;
using URMT.Core.Settings;
using URMT.Core.UI;
using URMT.Thunderstore.Windows;

namespace URMT.Thunderstore {
    public class ThunderstoreModuleSettings : SettingsSingleton<ThunderstoreModuleSettings> {
        public override string Name => "Thunderstore Module Settings";

        public bool isThunderstoreAPITokenSet;

        // The registry key is stored in HKEY_CURRENT_USER\Software\URMT\TemplateModuleSettings
        public static string ThunderstoreAPITokenRegistry {
            get => GetRegistryValue("ThunderstoreAPIToken", "");
            set => SetRegistryValue("ThunderstoreAPIToken", value);
        }

        private static string GetRegistryValue(string key, string defaultValue) {
            using(RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\URMT\TemplateModuleSettings")) {
                return registryKey?.GetValue(key, defaultValue)?.ToString() ?? defaultValue;
            }
        }

        private static void SetRegistryValue(string key, string value) {
            using(RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\URMT\TemplateModuleSettings")) {
                registryKey.SetValue(key, value);
            }
        }

        [RenderFor(nameof(isThunderstoreAPITokenSet))]
        public void RenderThunderstoreAPIToken(SerializedProperty property) {
            isThunderstoreAPITokenSet = !string.IsNullOrEmpty(ThunderstoreAPITokenRegistry);
            GUIUtils.RenderIndented(() => {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Thunderstore API Token:", GUILayout.Width(150));

                if(GUILayout.Button(isThunderstoreAPITokenSet ? "Change" : "Set", GUILayout.Width(80))) {
                    EditorWindow.GetWindow<SetThunderstoreAPITokenWindow>("Set Thunderstore API Token");
                }
                GUILayout.EndHorizontal();
            });
        }
    }
}
