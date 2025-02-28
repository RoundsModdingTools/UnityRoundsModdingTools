using UnityEditor;
using UnityEngine;
using System.IO;

namespace URMT.Utility {
    public static class CopyHandler {
        public static Object[] ObjectsToCopy { get; set; }

        public static void Paste(string destinationFolder) {
            if(ObjectsToCopy == null || ObjectsToCopy.Length == 0)
                return;

            // Ensure the destination folder ends with a slash
            if(!destinationFolder.EndsWith("/"))
                destinationFolder += "/";

            foreach(var obj in ObjectsToCopy) {
                // Get the asset path of the selected asset
                string sourcePath = AssetDatabase.GetAssetPath(obj);
                if(!string.IsNullOrEmpty(sourcePath)) {
                    // Retrieve the file name from the source path
                    string fileName = Path.GetFileName(sourcePath);
                    // Generate a unique destination path to avoid overwriting existing files
                    string destPath = AssetDatabase.GenerateUniqueAssetPath(destinationFolder + fileName);

                    // Copy the asset
                    if(!AssetDatabase.CopyAsset(sourcePath, destPath)) {
                        Debug.LogError("Failed to copy asset: " + sourcePath);
                    }
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
