using System.Collections;
using System.Collections.Generic;
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
            // These messages exist so you don't have to reference the `URMT.General` assembly in your own code
            MessageBus.RegisterMessage("AddModBundleMappings", args => {
                if(args.Length == 0 || args[0] == null) {
                    LoggerUtils.LogError("AddFolderMappings received no arguments!");
                    return;
                }

                if(!(args[0] is IEnumerable<(string, string)> collection)) {
                    LoggerUtils.LogError("AddFolderMappings received invalid argument type!");
                    return;
                }

                ModBundleMapping[] modBundleMappings = collection.Select(arg => {
                    return new ModBundleMapping(arg.Item1, arg.Item2);
                }).ToArray();

                // Only add mappings that don't already exist
                modBundleMappings = modBundleMappings
                    .Where(mapping => !GeneralModuleSettings.Instance.ModBundleMappings
                        .Any(existingMapping => existingMapping.ModName == mapping.ModName))
                    .ToArray();

                GeneralModuleSettings.Instance.ModBundleMappings.AddRange(modBundleMappings);
                EditorUtility.SetDirty(GeneralModuleSettings.Instance);

                LoggerUtils.Log($"Added {modBundleMappings.Length} mod bundle mappings");
            });

            MessageBus.RegisterMessage("AddFolderMappings", args => {
                if(args.Length == 0 || args[0] == null) {
                    LoggerUtils.LogError("AddFolderMappings received no arguments!");
                    return;
                }

                if(!(args[0] is IEnumerable<(string, string)> collection)) {
                    LoggerUtils.LogError("AddFolderMappings received invalid argument type!");
                    return;
                }

                FolderMapping[] folderMappings = collection.Select(arg => {
                    return new FolderMapping(arg.Item1, arg.Item2);
                }).ToArray();

                // Only add mappings that don't already exist
                folderMappings = folderMappings
                    .Where(mapping => !GeneralModuleSettings.Instance.FolderMappings
                        .Any(existingMapping => existingMapping.AssemblyName == mapping.AssemblyName))
                    .ToArray();

                GeneralModuleSettings.Instance.FolderMappings.AddRange(folderMappings);
                EditorUtility.SetDirty(GeneralModuleSettings.Instance);

                LoggerUtils.Log($"Added {folderMappings.Length} folder mappings");
            });

            MessageBus.RegisterMessage("RemoveModBundleMappings", args => {
                if(args.Length == 0 || args[0] == null) {
                    LoggerUtils.LogError("RemoveModBundleMappings received no arguments!");
                    return;
                }

                IEnumerable<string> collection;
                if(args[0] is string singleString) {
                    collection = new List<string> { singleString };
                } else if(args[0] is IEnumerable<object> objectCollection) {
                    collection = objectCollection.OfType<string>().ToList();
                } else {
                    LoggerUtils.LogError($"RemoveFolderMappings received invalid argument type: {args[0].GetType()}");
                    return;
                }


                GeneralModuleSettings.Instance.ModBundleMappings
                    .RemoveAll(mapping => collection.Contains(mapping.ModName));
                EditorUtility.SetDirty(GeneralModuleSettings.Instance);

                LoggerUtils.Log($"Removed {collection.Count()} mod bundle mappings");
            });

            MessageBus.RegisterMessage("RemoveFolderMappings", args => {
                if(args.Length == 0 || args[0] == null) {
                    LoggerUtils.LogError("RemoveFolderMappings received no arguments!");
                    return;
                }

                IEnumerable<string> collection;
                if(args[0] is string singleString) {
                    collection = new List<string> { singleString };
                } else if(args[0] is IEnumerable<object> objectCollection) {
                    collection = objectCollection.OfType<string>().ToList();
                } else {
                    LoggerUtils.LogError($"RemoveFolderMappings received invalid argument type: {args[0].GetType()}");
                    return;
                }

                GeneralModuleSettings.Instance.FolderMappings
                    .RemoveAll(mapping => collection.Contains(mapping.AssemblyName));
                EditorUtility.SetDirty(GeneralModuleSettings.Instance);

                LoggerUtils.Log($"Removed {collection.Count()} folder mappings");
            });

            LoggerUtils.Log("General Module Loaded");
        }
    }
}