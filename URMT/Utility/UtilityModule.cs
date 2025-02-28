using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;

namespace URMT.Utility {
    [URMTModule("Utility", "com.aalund13.urmt.utility")]
    [URMTModuleDependency("com.aalund13.urmt.core")]
    public class UtilityModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[0];

        public void OnModuleLoad() {
            LoggerUtils.Log("Utility module loaded");
        }
    }
}
