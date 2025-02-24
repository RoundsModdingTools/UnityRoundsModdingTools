using System.Linq;
using URMT.Core.Managers;
using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;
using URMT.General.Entities;

namespace URMT.General {
    [URMTModule("General", "com.aalund13.urmt.general")]
    [URMTModuleDependency("com.aalund13.urmt.core")]
    public class GeneralModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[] { GeneralModuleSettings.Instance };

        public void OnModuleLoad() {
            // These messages exist so you don't have to reference the `URMT.General` assembly in your own code
            MessageBus.RegisterMessage("AddModBundleMappings", args => {
                ModBundleMapping[] modBundleMappings = args.Select(arg => {
                    (string modName, string assetBundleName) = ((string, string))arg;
                    return new ModBundleMapping(modName, assetBundleName);
                }).ToArray();

                // Only add mappings that don't already exist
                modBundleMappings = modBundleMappings
                    .Where(mapping => !GeneralModuleSettings.Instance.ModBundleMappings
                        .Any(existingMapping => existingMapping.ModName == mapping.ModName))
                    .ToArray();

                GeneralModuleSettings.Instance.ModBundleMappings.AddRange(modBundleMappings);

                LoggerUtils.Log($"Added {modBundleMappings.Length} mod bundle mappings");
            });

            MessageBus.RegisterMessage("AddFolderMappings", args => {
                FolderMapping[] folderMappings = args.Select(arg => {
                    (string assemblyName, string folderName) = ((string, string))arg;
                    return new FolderMapping(assemblyName, folderName);
                }).ToArray();

                // Only add mappings that don't already exist
                folderMappings = folderMappings
                    .Where(mapping => !GeneralModuleSettings.Instance.FolderMappings
                        .Any(existingMapping => existingMapping.FolderName == mapping.FolderName))
                    .ToArray();

                GeneralModuleSettings.Instance.FolderMappings.AddRange(folderMappings);

                LoggerUtils.Log($"Added {folderMappings.Length} folder mappings");
            });

            LoggerUtils.Log("General Module Loaded");
        }
    }
}