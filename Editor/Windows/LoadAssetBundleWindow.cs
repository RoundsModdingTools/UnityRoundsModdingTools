using UnityEditor;
using UnityEngine;
using UnityRoundsModdingTools.Editor.Utils;

namespace UnityRoundsModdingTools.Editor.Windows {
    public class LoadAssetBundleWindow : EditorWindow {
        private DefaultAsset selectedAssetBundle;
        private AssetBundle loadedAssetBundle;
        private Object[] loadedAssets;

        private static string assetBundlePath;


        [MenuItem("Unity Rounds Modding Tools/AssetBundle Loader")]
        private static void ShowWindow() {
            GetWindow(typeof(LoadAssetBundleWindow), false, "AssetBundle Loader");
        }

        private void OnGUI() {
            GUIUtils.DrawTitle("AssetBundle Loader");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Select AssetBundle:");
            selectedAssetBundle = EditorGUILayout.ObjectField(selectedAssetBundle, typeof(DefaultAsset), false) as DefaultAsset;
            GUILayout.EndHorizontal();

            if(selectedAssetBundle != null) {
                assetBundlePath = AssetDatabase.GetAssetPath(selectedAssetBundle);
                EditorGUILayout.LabelField("AssetBundle Path:", assetBundlePath);

                if(GUILayout.Button("Load AssetBundle")) {
                    AssetBundle.UnloadAllAssetBundles(false);

                    loadedAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);

                    if(loadedAssetBundle != null) {
                        UnityEngine.Debug.Log("AssetBundle loaded successfully from path: " + assetBundlePath);

                        loadedAssets = loadedAssetBundle.LoadAllAssets();
                        if(loadedAssets != null && loadedAssets.Length > 0) {
                            UnityEngine.Debug.Log("Loaded Assets:");
                            foreach(Object loadedAsset in loadedAssets) {
                                UnityEngine.Debug.Log(loadedAsset.name + " (" + loadedAsset.GetType() + ")");
                                Instantiate(loadedAsset);
                            }
                        } else {
                            UnityEngine.Debug.Log("No assets found in the selected AssetBundle.");
                        }
                    } else {
                        UnityEngine.Debug.LogError("Failed to load AssetBundle from path: " + assetBundlePath);
                    }
                }
            }
        }
    }
}
