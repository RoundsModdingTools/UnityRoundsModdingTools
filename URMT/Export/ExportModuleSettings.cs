using UnityEngine;
using URMT.Core.ScriptableObjects;

namespace URMT.Export {
    public class ExportModuleSettings : SettingsSingleton<ExportModuleSettings> {
        public override string Name => "Export Module Settings";

        public string ExportPath = "ExportedMods";
        [Tooltip("If not empty, the export folder will be copied to the specified folder after export.")]
        public string ExportFolderCopyTo = "";

        public bool AutoExport = false;
    }
}
