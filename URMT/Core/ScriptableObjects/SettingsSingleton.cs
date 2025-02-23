using UnityEngine;
using URMT.Core.Attributes;
using URMT.Core.Settings;

namespace URMT.Core.ScriptableObjects {
    [ScriptableSingletonPath("Assets/Resources/UnityRoundsModdingTools/Settings")]
    public abstract class SettingsSingleton<T> : ScriptableSingleton<T>, ISettingMenu where T : ScriptableObject {
        public abstract string Name { get; }
    }
}
