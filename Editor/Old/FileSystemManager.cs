using System.IO;
using System.Linq;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor {
    public static class FileSystemManager {
        private static Settings Settings => Settings.Instance;

        public static string DllsFolderPath => Path.Combine(Application.dataPath, Settings.DllsFolderPath);
        public static string ModsFolderPath => Path.Combine(Application.dataPath, Settings.ModsFolderPath);
        public static string BepinexAndHarmonyFolderPath => Path.Combine(Application.dataPath, Settings.BepinexFolderPath);

        public static void CopyDirectory(string sourceDirPath, string destDirPath, string[] blacklistedFileExtension = null, string[] blacklistedDirectory = null) {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirPath);
            DirectoryInfo destDir = new DirectoryInfo(destDirPath);

            if(!destDir.Exists) {
                destDir.Create();
            }

            foreach(FileInfo file in sourceDir.GetFiles()) {
                bool isBlacklisted = blacklistedFileExtension != null ? blacklistedFileExtension.Contains(file.Extension.Replace(".", "")) : Settings.BlacklistedFileExtension.Contains(file.Extension.Replace(".", ""));
                if(isBlacklisted) continue;

                string destFilePath = Path.Combine(destDir.FullName, file.Name);
                file.CopyTo(destFilePath, true);
            }

            foreach(DirectoryInfo subDir in sourceDir.GetDirectories()) {
                bool isBlacklisted = blacklistedFileExtension != null ? blacklistedDirectory.Contains(subDir.Name) : Settings.BlacklistedDirectory.Contains(subDir.Name);
                if(isBlacklisted) continue;

                string destSubDirPath = Path.Combine(destDir.FullName, subDir.Name);
                CopyDirectory(subDir.FullName, destSubDirPath, blacklistedFileExtension, blacklistedDirectory);
            }
        }
    }
}
