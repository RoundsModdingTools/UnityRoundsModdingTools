using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using URMT.Core.Modules;

namespace URMT.Core.Managers {
    [InitializeOnLoad]
    public static class ModuleManager {
        private static List<ModuleInfo> Modules { get; } = new List<ModuleInfo>();
        public static IReadOnlyList<ModuleInfo> LoadedModules => Modules.AsReadOnly();

        static ModuleManager() {
            LoadModules();
        }

        public static void LoadModules() {
            Modules.Clear();

            // load all assemblies that reference URMT.Core
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetReferencedAssemblies().Any(y => y.Name == "URMT.Core"))
                .ToList();

            assemblies.Add(typeof(ModuleManager).Assembly);

            // Populate modules list with all 'IModuleEntry' instances
            foreach(var assembly in assemblies) {
                foreach(var type in assembly.GetTypes()) {
                    if(type.GetInterfaces().Contains(typeof(IModuleEntry))) {
                        var module = (IModuleEntry)Activator.CreateInstance(type);

                        string error = CheckIfValidModule(type);
                        if(error != null) {
                            UnityEngine.Debug.LogWarning($"Failed to load module {type.Name}: {error}");
                            continue;
                        }

                        Modules.Add(new ModuleInfo(module));
                    }
                }
            }

            // Sort the modules list by dependencies
            for(int i = 0; i < Modules.Count; i++) {
                for(int j = i + 1; j < Modules.Count; j++) {
                    if(Modules[i].Dependencies.Contains(Modules[j].ID)) {
                        var temp = Modules[i];
                        Modules[i] = Modules[j];
                        Modules[j] = temp;
                    }
                }
            }

            // Load the modules
            foreach(var module in Modules) {
                try {
                    module.ModuleEntry.OnModuleLoad();

                    foreach(var settingMenu in module.ModuleEntry.SettingMenus) {
                        SettingsManager.RegisterSettingMenu(settingMenu);
                    }
                } catch(Exception e) {
                    UnityEngine.Debug.LogError($"Failed to load module {module.Name}: {e}");
                }
            }

        }

        private static string CheckIfValidModule(Type type) {
            var attributes = type.GetCustomAttributes(typeof(URMTModuleAttribute), false);
            if(type == null)
                return "Type is null";
            else if(!type.GetInterfaces().Contains(typeof(IModuleEntry)))
                return $"{type.Name} does not implement IModuleEntry";
            else if(attributes.Length == 0)
                return $"{type.Name} does not have URMTModuleAttribute";
            else {
                var guid = ((URMTModuleAttribute)attributes.First()).GUID;
                if(Modules.Any(x => x.ID == guid))
                    return $"{type.Name} has a duplicate GUID: {guid}";
            }
            return null;
        }

    }
}
