using URMT.Core.ScriptableObjects;
using URMT.Core.Settings;

namespace URMT.Core.Modules {
    public interface IModuleEntry {
        ISettingMenu[] SettingMenus { get; }
        void OnModuleLoad();
    }

    // Test module, Will be removed in the future
    [URMTModule("Module Name", "com.example.module", "1.0.0")]
    public class TestModule : SettingsSingleton<TestModule>, IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[1] { TestModule.Instance };
        public override string Name => "Module Name";


        public string TestSetting = "Test";
        public int TestIntSetting = 5;


        public void OnModuleLoad() {
            UnityEngine.Debug.Log("Module loaded!");
        }
    }
}
