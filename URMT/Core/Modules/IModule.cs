using URMT.Core.ScriptableObjects;
using URMT.Core.Settings;

namespace URMT.Core.Modules {
    public interface IModuleEntry {
        ISettingMenu[] SettingMenus { get; }
        void OnModuleLoad();
    }
}
