using UnityEditor;
using UnityEngine;
using URMT.Core.UI;

namespace URMT.Thunderstore.Windows {
    public class SetThunderstoreAPITokenWindow : EditorWindow {
        private string thunderstoreAPIToken;

        public void OnGUI() {
            GUIUtils.DrawTitle("Set Thunderstore API Token");
            EditorGUILayout.LabelField("Enter your Thunderstore API Token below, Once set it cannot be viewed again, This is used to publish mods to Thunderstore", new GUIStyle(GUI.skin.label) { wordWrap = true });

            thunderstoreAPIToken = EditorGUILayout.PasswordField("Thunderstore API Token:", thunderstoreAPIToken);
            if(GUILayout.Button("Set API Token")) {
                ThunderstoreModuleSettings.ThunderstoreAPITokenRegistry = thunderstoreAPIToken;
                Close();
            }
        }
    }
}
