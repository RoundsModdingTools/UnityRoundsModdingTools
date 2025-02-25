using URMT.Core.ScriptableObjects;
using URMT.Core.Settings;

namespace URMT.Template {
    public class TemplateModuleSettings : SettingsSingleton<TemplateModuleSettings> {
        public override string Name => "Template Module Settings";

        public string ModTemplateExportPath = "Assets";
        public string ModTemplateImportPath = "https://github.com/RoundsModdingTools/UnityRoundsModTemplate";
    }
}