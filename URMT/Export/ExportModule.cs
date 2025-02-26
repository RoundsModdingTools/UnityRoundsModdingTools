using System;
using URMT.Core.Managers;
using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;
using URMT.Export.Networking;
using URMT.Export.ScriptableObjects;

namespace URMT.Export {
    [URMTModule("Export", "com.aalund13.urmt.export")]
    [URMTModuleDependency("com.aalund13.urmt.core")]
    public class ExportModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[] { ExportModuleSettings.Instance };
        public const int MESSAGE_SERVER_PORT = 37189;

        public void OnModuleLoad() {
            MessageBus.RegisterMessage("ExportAssembly", (args) => {
                if(args.Length == 0 || args[0] == null) {
                    LoggerUtils.LogError("ExportAssembly received no arguments!");
                    return;
                }

                string assemblyName = args[0] as string;
                ModInfo.ExportAssembly(assemblyName);
            });

            MessageBus.RegisterMessage("ExportAssemblyAuto", (args) => {
                if(args.Length == 0 || args[0] == null) {
                    LoggerUtils.LogError("ExportAssembly received no arguments!");
                    return;
                } else if(!ExportModuleSettings.Instance.AutoExport) {
                    return;
                }

                string assemblyName = args[0] as string;
                ModInfo.ExportAssembly(assemblyName);
            });

            var MessageServer = new MessageServer("127.0.0.1", MESSAGE_SERVER_PORT);

            // Stop the server when the domain is unloaded
            AppDomain.CurrentDomain.DomainUnload += (sender, e) => {
                MessageServer.StopServer();
            };


            LoggerUtils.Log("ExportModule loaded");
        }
    }
}