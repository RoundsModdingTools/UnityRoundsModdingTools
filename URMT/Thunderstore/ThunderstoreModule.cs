using Microsoft.Win32;
using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;

namespace URMT.Thunderstore {
    [URMTModule("Thunderstore", "com.aalund13.urmt.thunderstore")]
    public class ThunderstoreModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[] { ThunderstoreModuleSettings.Instance };

        public void OnModuleLoad() {
            LoggerUtils.Log("ThunderstoreModule loaded");
        }
    }
}
