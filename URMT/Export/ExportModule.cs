using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;
using URMT.Export.ScriptableObjects;
using URMT.Networking;

namespace URMT.Export {
    [URMTModule("Export", "com.aalund13.urmt.export")]
    [URMTModuleDependency("com.aalund13.urmt.core")]
    [URMTModuleDependency("com.aalund13.urmt.networking")]
    public class ExportModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[] { ExportModuleSettings.Instance };

        public void OnModuleLoad() {
            MessageServer.RegisterMessage("ExportAssembly", (args) => {
                if(args.Length == 0 || args[0] == null) {
                    LoggerUtils.LogError("ExportAssembly received no arguments!");
                    return;
                }

                string assemblyName = args[0] as string;
                ModInfo.ExportAssembly(assemblyName);
            });

            MessageServer.RegisterMessage("ExportAssemblyAuto", (args) => {
                if(args.Length == 0 || args[0] == null) {
                    LoggerUtils.LogError("ExportAssembly received no arguments!");
                    return;
                } else if(!ExportModuleSettings.Instance.AutoExport) {
                    return;
                }

                string assemblyName = args[0] as string;
                ModInfo.ExportAssembly(assemblyName);
            });

            NetworkingModule.Server.OnServerStarted += (server) => {
                ModInfo.GenerateAllScripts();
            };

            LoggerUtils.Log("ExportModule loaded");
        }
    }
}