using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.Utils {
    public static class GithubUtils {
        private static Settings Settings => Settings.Instance;

        public static List<string> DownloadGithubProject(string githubURL) {
            string[] ownerAndRepo = githubURL.Replace("https://github.com/", "").Split('/');

            if(!IsValidGithubUrl(githubURL)) {
                UnityEngine.Debug.LogWarning("Invalid GitHub URL.");
                return null;
            }

            return DownloadAndInstallProject($"https://api.github.com/repos/{ownerAndRepo[0]}/{ownerAndRepo[1]}/zipball", $"{ownerAndRepo[1]}.zip");
        }

        private static List<string> DownloadAndInstallProject(string url, string fileName) {
            string tempFolderPath = Path.Combine(Settings.TempPath, fileName.Replace(".zip", ""));
            string savePath = Path.Combine(Settings.TempPath, fileName);

            if(Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            if(!Directory.Exists(Settings.TempPath)) Directory.CreateDirectory(Settings.TempPath);

            UnityEngine.Debug.Log($"Downloading project from {url}...");
            UnityEngine.Debug.Log($"Saving to {savePath}");
            try {
                using(WebClient client = new WebClient()) {
                    client.Headers.Add("User-Agent", "request");

                    client.DownloadFile(url, savePath);
                    ZipFile.ExtractToDirectory(savePath, tempFolderPath);

                    var assemblyNames = ProjectUtils.ConvertToUnityProject(tempFolderPath);
                    Directory.Delete(tempFolderPath, true);
                    return assemblyNames;
                }
            } catch(Exception ex) {
                UnityEngine.Debug.LogError($"Error while downloading zip file: {ex.Message}");
            }

            return null;
        }

        public static bool IsValidGithubUrl(string urlPath) {
            string[] ownerAndRepo = urlPath.Replace("https://github.com/", "").Split('/');

            if(ownerAndRepo.Length != 2) {
                return false;
            }
            return true;
        }
    }
}
