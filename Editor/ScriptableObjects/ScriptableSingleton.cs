using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityRoundsModdingTools.Editor.ScriptableObjects {
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject {
        private static T instance;

        public static T Instance {
            get {
                if(instance == null) Load();
                if(instance == null) {
                    Debug.LogWarning($"No instance of {typeof(T).Name} was found in the project. Creating a new instance.");
                    CreateAndLoad();
                }
                return instance;
            }
        }

        private static void Load() {
            string[] guilds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach(var guild in guilds) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guild);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if(asset != null) {
                    instance = asset;
                    break;
                }
            }
        }

        private static void CreateAndLoad() {
            instance = ScriptableObject.CreateInstance<T>();

            // Save the newly created instance as an asset
            string path = $"Assets/Resources/UnityRoundsModdingTools/{typeof(T).Name}.asset";
            if(!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));

            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created and saved new instance of {typeof(T).Name} at {path}");
        }

        public static void Save() {
            if(instance == null) {
                Debug.LogError($"Cannot save {typeof(T).Name} instance because it is null.");
                return;
            }

            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
        }
    }
}
