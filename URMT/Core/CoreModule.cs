using System.IO;
using UnityEngine;
using URMT.Core.Modules;
using URMT.Core.ScriptableObjects;
using URMT.Core.Settings;
using URMT.Core.Utils;

namespace URMT.Core {
    [URMTModule("Core Module", "com.aalund13.urmt.core")]
    public class CoreModule : SettingsSingleton<CoreModule>, IModuleEntry {
        public override string Name => "Core Module Settings";

        public ISettingMenu[] SettingMenus => new ISettingMenu[] { CoreModule.Instance };

        [Header("General Settings")]
        public string DllsFolderPath = Path.Combine("Assets", "Scripts", "Dlls");
        public string ModsFolderPath = Path.Combine("Assets", "Scripts", "Mods");
        public string TempPath = Path.Combine(Path.GetTempPath(), "UnityRoundsModdingTools");

        [Header("Debug Settings")]
        public bool EnableDebugLogging = false;

        public void OnModuleLoad() {
            LoggerUtils.Log("Core Module Loaded");
        }
    }
}
