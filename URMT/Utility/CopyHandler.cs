using UnityEditor;
using UnityEngine;
using System.IO;

namespace URMT.Utility {
    public static class CopyHandler {
        public static Object[] ObjectsToCopy { get; set; }

        public static void Paste(string destinationFolder) {
            if(ObjectsToCopy == null || ObjectsToCopy.Length == 0)
                return;

            if(!destinationFolder.EndsWith("/"))
                destinationFolder += "/";

            foreach(var obj in ObjectsToCopy) {
                string sourcePath = AssetDatabase.GetAssetPath(obj);
                if(!string.IsNullOrEmpty(sourcePath)) {
                    string fileName = Path.GetFileName(sourcePath);
                    string destPath = AssetDatabase.GenerateUniqueAssetPath(destinationFolder + fileName);

                    if(!AssetDatabase.CopyAsset(sourcePath, destPath)) {
                        Debug.LogError("Failed to copy asset: " + sourcePath);
                    }
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
