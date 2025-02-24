using URMT.Core.Modules;
using URMT.Core.Settings;

namespace URMT.General {
    [URMTModule("General", "com.aalund13.urmt.general")]
    public class GeneralModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[1] { GeneralModuleSettings.Instance };

        public void OnModuleLoad() {
            UnityEngine.Debug.Log("General Module loaded!");
        }
    }
}