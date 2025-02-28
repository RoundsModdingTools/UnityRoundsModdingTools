using UnityEditor;
using URMT.Core.Managers;
using URMT.Core.ScriptableObjects;
using URMT.Core.Settings;

namespace URMT.Core.CustomInspectors {
    [CustomEditor(typeof(SettingsSingleton<>), true)]
    internal class SettingsSingletonInspector : Editor {
        public override void OnInspectorGUI() {
            SettingsManager.RenderSettings((ISettingMenu)target);
        }
    }
}
