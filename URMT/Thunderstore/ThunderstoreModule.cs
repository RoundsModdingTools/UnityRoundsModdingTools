using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;

namespace URMT.Thunderstore {
    [URMTModule("Thunderstore", "com.aalund13.urmt.thunderstore")]
    public class ThunderstoreModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[0];

        public void OnModuleLoad() {
            LoggerUtils.Log("ThunderstoreModule loaded");
        }
    }
}
