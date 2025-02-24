using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;

namespace URMT.ModManager {
    [URMTModule("ModManager", "com.aalund13.urmt.modmanager")]
    public class ModManagerModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[] { ModManagerModuleSettings.Instance };

        public void OnModuleLoad() {
            LoggerUtils.Log("ModManager Module Loaded");
        }
    }
}