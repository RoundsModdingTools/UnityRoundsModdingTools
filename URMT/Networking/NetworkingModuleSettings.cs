using UnityEditor;
using UnityEngine;
using URMT.Core.ScriptableObjects;
using URMT.Core.Settings;
using URMT.Core.UI;

namespace URMT.Networking {
    public class NetworkingModuleSettings : SettingsSingleton<NetworkingModuleSettings> {
        public override string Name => "Networking Module Settings";

        public ushort Port = 37189;

        [RenderFor(nameof(Port))]
        private void RenderPort(SerializedProperty serializedProperty) {
            EditorGUILayout.PropertyField(serializedProperty);
            GUIUtils.RenderIndented(() => {
                if(GUILayout.Button("Restart Server")) {
                    NetworkingModule.Server.RestartServer("127.0.0.1", Port);
                }
            });
        }
    }
}
