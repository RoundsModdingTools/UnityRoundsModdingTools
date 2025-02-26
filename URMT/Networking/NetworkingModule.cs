using System;
using URMT.Core.Modules;
using URMT.Core.Settings;
using URMT.Core.Utils;

namespace URMT.Networking {
    [URMTModule("Networking", "com.aalund13.urmt.networking")]
    public class NetworkingModule : IModuleEntry {
        public ISettingMenu[] SettingMenus => new ISettingMenu[] { NetworkingModuleSettings.Instance };
        public static MessageServer Server { get; private set; }

        public void OnModuleLoad() {
            LoggerUtils.Log("Networking Module Loaded");
            Server = new MessageServer("127.0.0.1", NetworkingModuleSettings.Instance.Port);

            AppDomain.CurrentDomain.DomainUnload += (sender, e) => {
                Server.StopServer();
            };
        }
    }
}
