using System.IO;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor {
    public static class FileSystemManager {
        private static Settings Settings => Settings.Instance;

        public static string DllsFolderPath => Path.Combine(Application.dataPath, Settings.DllsFolderPath);
        public static string ModsFolderPath => Path.Combine(Application.dataPath, Settings.ModsFolderPath);
        public static string BepinexAndHarmonyFolderPath => Path.Combine(Application.dataPath, Settings.BepinexAndHarmonyFolderPath);

        public static void CopyDirectory(string sourceDirPath, string destDirPath) {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirPath);
            DirectoryInfo destDir = new DirectoryInfo(destDirPath);

            if(!destDir.Exists) {
                destDir.Create();
            }

            foreach(FileInfo file in sourceDir.GetFiles()) {
                if(Settings.BlacklistedFileExtension.Contains(file.Extension.Replace(".", ""))) continue;

                string destFilePath = Path.Combine(destDir.FullName, file.Name);
                file.CopyTo(destFilePath, true);
            }
            
            foreach(DirectoryInfo subDir in sourceDir.GetDirectories()) {
                if(Settings.BlacklistedDirectory.Contains(subDir.Name)) continue;

                string destSubDirPath = Path.Combine(destDir.FullName, subDir.Name);
                CopyDirectory(subDir.FullName, destSubDirPath);
            }
        }
    }
}
