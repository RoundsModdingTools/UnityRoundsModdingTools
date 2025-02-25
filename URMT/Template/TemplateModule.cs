using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;
using URMT.Template.Templates;

namespace URMT.Template {
    [URMTModule("Template", "TemplateModule")]
    [URMTModuleDependency("com.aalund13.urmt.core")]
    [URMTModuleDependency("com.aalund13.urmt.general")]
    public class TemplateModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[] { TemplateModuleSettings.Instance };

        public void OnModuleLoad() {
            TemplateManager.RegisterTemplateHandler("ModTemplate", new ModTemplateHandler());

            LoggerUtils.Log("TemplateModule loaded");
        }
    }
}