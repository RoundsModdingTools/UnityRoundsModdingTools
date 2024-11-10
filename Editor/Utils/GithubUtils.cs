using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.Utils {
    public static class GithubUtils {
        public static List<string> InstallGithubProject(string githubUrl) {
            if(!IsValidGithubUrl(githubUrl)) {
                UnityEngine.Debug.LogWarning("Invalid GitHub URL.");
                return null;
            }

            string[] ownerAndRepo = ExtractOwnerAndRepo(githubUrl);
            string fileName = $"{ownerAndRepo[1]}";

            string savePath = DownloadGithubProject(githubUrl, fileName);
            if(savePath == null) {
                UnityEngine.Debug.LogError("Failed to download the project.");
                return null;
            }

            var assemblyNames = ProjectUtils.ConvertToUnityProject(savePath);
            Directory.Delete(Settings.Instance.TempPath, true);

            return assemblyNames;
        }

        public static string DownloadGithubProject(string githubUrl, string fileName) {
            string[] ownerAndRepo = ExtractOwnerAndRepo(githubUrl);
            string downloadUrl = $"https://api.github.com/repos/{ownerAndRepo[0]}/{ownerAndRepo[1]}/zipball";
            string savePath = Path.Combine(Settings.Instance.TempPath, fileName);

            try {
                PrepareDirectory(Settings.Instance.TempPath, savePath);

                UnityEngine.Debug.Log($"Downloading project from {githubUrl}...");

                using(var client = new WebClient()) {
                    client.Headers.Add("User-Agent", "UnityRoundsModdingTools");
                    client.DownloadFile(downloadUrl, $"{savePath}.zip");
                    ZipFile.ExtractToDirectory($"{savePath}.zip", savePath);

                    DirectoryInfo modDirectory = new DirectoryInfo(savePath);
                    modDirectory = modDirectory.GetDirectories().First();
                    return modDirectory.FullName;
                }
            } catch(WebException webEx) {
                UnityEngine.Debug.LogError($"Web error during download: {webEx.Message}");
            } catch(IOException ioEx) {
                UnityEngine.Debug.LogError($"File I/O error: {ioEx.Message}");
            } catch(Exception ex) {
                UnityEngine.Debug.LogError($"Unexpected error: {ex.Message}");
            }

            return null;
        }

        public static bool IsValidGithubUrl(string url) {
            string pattern = @"^https://github\.com/([^/]+/[^/]+)$";
            return Regex.IsMatch(url, pattern);
        }

        private static string[] ExtractOwnerAndRepo(string githubUrl) {
            return githubUrl.Replace("https://github.com/", "").Split('/');
        }

        private static void PrepareDirectory(string tempFolderPath, string tempModFolderPath) {
            if(!Directory.Exists(tempFolderPath)) Directory.CreateDirectory(tempFolderPath);
            if(Directory.Exists(tempModFolderPath)) Directory.Delete(tempModFolderPath, true);
        }
    }
}
