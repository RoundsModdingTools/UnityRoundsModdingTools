using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;

namespace URMT.ModManager {
    [URMTModule("ModManager", "com.aalund13.urmt.modmanager")]
    [URMTModuleDependency("com.aalund13.urmt.core")]
    [URMTModuleDependency("com.aalund13.urmt.general")]
    public class ModManagerModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[0];

        public void OnModuleLoad() {
            LoggerUtils.Log("ModManager Module Loaded");
        }
    }
}