using System.IO;
using System.Linq;

namespace URMT.Core.Utils {
    public static class FileSystemUtils {
        public static void CopyDirectory(string sourceDirPath, string destDirPath, string[] blacklistedFileExtension = null, string[] blacklistedDirectory = null) {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirPath);
            DirectoryInfo destDir = new DirectoryInfo(destDirPath);

            blacklistedDirectory = blacklistedDirectory == null ? new string[0] : blacklistedDirectory;
            blacklistedFileExtension = blacklistedFileExtension == null ? new string[0] : blacklistedFileExtension.Select(ext => ext.Replace(".", "")).ToArray();

            if(destDir.Exists) {
                destDir.Delete(true);
            }
            destDir.Create();

            foreach(FileInfo file in sourceDir.GetFiles()) {
                bool isBlacklisted = blacklistedFileExtension.Contains(file.Extension.Replace(".", ""));
                if(isBlacklisted) continue;

                string destFilePath = Path.Combine(destDir.FullName, file.Name);
                file.CopyTo(destFilePath, true);
            }

            foreach(DirectoryInfo subDir in sourceDir.GetDirectories()) {
                bool isBlacklisted = blacklistedDirectory.Contains(subDir.Name);
                if(isBlacklisted) continue;

                string destSubDirPath = Path.Combine(destDir.FullName, subDir.Name);
                CopyDirectory(subDir.FullName, destSubDirPath, blacklistedFileExtension, blacklistedDirectory);
            }
        }
    }
}
