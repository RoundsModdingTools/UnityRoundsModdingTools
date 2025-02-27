using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
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
            LoggerUtils.Log("General Module Loaded");
        }

        public static void RemoveFolderMapping(string assemblyName) {
            GeneralModuleSettings.Instance.FolderMappings
                .RemoveAll(mapping => mapping.AssemblyName == assemblyName);

            EditorUtility.SetDirty(GeneralModuleSettings.Instance);
        }

        public static void AddFolderMapping(string assemblyName, string folderName) {
            if(GeneralModuleSettings.Instance.FolderMappings.Any(mapping => mapping.AssemblyName == assemblyName)) {
                LoggerUtils.LogWarning($"Folder mapping for {assemblyName} already exists");
                return;
            }

            GeneralModuleSettings.Instance.FolderMappings.Add(new FolderMapping(assemblyName, folderName));
            EditorUtility.SetDirty(GeneralModuleSettings.Instance);
        }

        public static void RemoveModBundleMapping(string modName) {
            GeneralModuleSettings.Instance.ModBundleMappings
                .RemoveAll(mapping => mapping.ModName == modName);

            EditorUtility.SetDirty(GeneralModuleSettings.Instance);
        }

        public static void AddModBundleMapping(string modName, string bundleName) {
            if(GeneralModuleSettings.Instance.ModBundleMappings.Any(mapping => mapping.ModName == modName)) {
                LoggerUtils.LogWarning($"Mod bundle mapping for {modName} already exists");
                return;
            }
            GeneralModuleSettings.Instance.ModBundleMappings.Add(new ModBundleMapping(modName, bundleName));
            EditorUtility.SetDirty(GeneralModuleSettings.Instance);
        }
    }
}