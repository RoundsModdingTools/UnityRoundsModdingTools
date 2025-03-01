using GitHubAPI;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ThunderstoreAPI;
using ThunderstoreAPI.Entities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using URMT.Core;
using URMT.Core.UI;
using URMT.Core.Utils;
using URMT.ModManager;

namespace Thunderstore.Windows {
    public enum PackageSortType {
        LastUpdated,
        Newest,
        MostDownloaded,
        TopRated,
    }

    internal class ThunderstoreBrowserWindow : EditorWindow {
        private const string COMMUNITY = "rounds";

        private static Package[] packages = new Package[0];
        private static Package[] shownPackages = new Package[0];

        private static Category[] categories = new Category[0];
        private static bool[] selectedWhitelistedCategories = new bool[0];
        private static bool[] selectedBlacklistedCategories = new bool[0];

        private static string searchQuery = "";
        private PackageSortType sortType;

        private static ThunderstoreApiClient client;
        private static ReorderableList modList;
        private Vector2 scrollPos;


        [MenuItem("URMT/Thunderstore/Thunderstore Browser")]
        public static void ShowWindow() {
            var window = GetWindow<ThunderstoreBrowserWindow>("Thunderstore Browser");
            window.minSize = new Vector2(700, 300);
        }

        private async void OnEnable() {
            // Grab the packages on enable (async)
            packages = new Package[0];
            shownPackages = new Package[0];

            SetupThunderstoreClient();

            await GetPackages();
        }

        private void OnGUI() {
            GUIUtils.DrawTitle("Thunderstore Browser");
            if(modList == null) CreateModList();
            SetupThunderstoreClient();

            searchQuery = EditorGUILayout.TextField("Search:", searchQuery);
            sortType = (PackageSortType)EditorGUILayout.EnumPopup("Sort by:", sortType);

            if(packages.Length == 0) {
                EditorGUILayout.HelpBox("Loading packages...", MessageType.Info);
                return;
            }

            GUIUtils.CreateMultSelectDropdown("Whitelist Categories:", "Categories", categories.Select(x => x.Name).ToList(), selectedWhitelistedCategories);
            GUILayout.Space(1);
            GUIUtils.CreateMultSelectDropdown("Blacklist Categories:", "Categories", categories.Select(x => x.Name).ToList(), selectedBlacklistedCategories);
            GUILayout.Space(5);

            shownPackages = GetShowenPackage();
            modList.list = shownPackages;

            using(var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos)) {
                scrollPos = scrollView.scrollPosition;
                modList?.DoLayoutList();
            }
        }

        private Package[] GetShowenPackage() {
            Package[] sortedPackages = OrderBySortType(packages, sortType);
            return sortedPackages
                .Where(x => x.FullName.ToLower()
                .Contains(searchQuery.ToLower()))
                .Where(x => !selectedWhitelistedCategories.Any(s => s) || x.Categories.Any(c => selectedWhitelistedCategories[Array.IndexOf(categories.Select(cat => cat.Name).ToArray(), c)]))
                .Where(x => !selectedBlacklistedCategories.Any(s => s) || !x.Categories.Any(c => selectedBlacklistedCategories[Array.IndexOf(categories.Select(cat => cat.Name).ToArray(), c)]))
                .Take(100)
                .ToArray();
        }

        private Package[] OrderBySortType(Package[] packages, PackageSortType packageSortType, bool descending = true) {
            Package[] sortedPackages = packages;
            switch(packageSortType) {
                case PackageSortType.LastUpdated:
                    sortedPackages = packages.OrderByDescending(x => x.DateUpdated).ToArray();
                    break;
                case PackageSortType.Newest:
                    sortedPackages = packages.OrderByDescending(x => x.DateCreated).ToArray();
                    break;
                case PackageSortType.MostDownloaded:
                    sortedPackages = packages.OrderByDescending(x => x.Versions[0].Downloads).ToArray();
                    break;
                case PackageSortType.TopRated:
                    sortedPackages = packages.OrderByDescending(x => x.RatingScore).ToArray();
                    break;
            }

            return descending ? sortedPackages : sortedPackages.Reverse().ToArray();
        }

        private async Task GetPackages() {
            packages = (await client.GetPackagesAsync(COMMUNITY))
                .Where(x => x.FullName != "ebkr-r2modman")
                .OrderByDescending(x => x.IsPinned)
                .ToArray();

            shownPackages = GetShowenPackage();

            // If the list has already been created, update its reference:
            if(modList != null) {
                modList.list = shownPackages;
            }

            Repaint();
        }

        private void CreateModList() {
            modList = new ReorderableList(shownPackages, typeof(Package),
                draggable: false, displayHeader: true, displayAddButton: false, displayRemoveButton: false);

            modList.drawHeaderCallback = rect => {
                GUI.Label(rect, "Mods");
            };

            // Adjust element height so everything fits
            modList.elementHeight = EditorGUIUtility.singleLineHeight + 6f;

            modList.drawElementCallback = (rect, index, active, focused) => {
                var package = shownPackages[index];

                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                // Define button sizes
                float spacing = 5f;
                float buttonWidth1 = 130f;
                float buttonWidth2 = 50f;
                float buttonWidth3 = 45f;
                float totalButtonWidth = buttonWidth1 + buttonWidth2 + buttonWidth3 + (spacing * 2);

                float contentWidth = rect.width - totalButtonWidth - spacing;

                float modNameWidth = contentWidth * 0.6f;
                float ownerWidth = contentWidth * 0.4f;

                Rect modNameRect = new Rect(rect.x, rect.y, modNameWidth, rect.height);

                Rect buttonRect1 = new Rect(rect.xMax - totalButtonWidth, rect.y, buttonWidth1, rect.height);
                Rect buttonRect2 = new Rect(buttonRect1.xMax + spacing, rect.y, buttonWidth2, rect.height);
                Rect buttonRect3 = new Rect(buttonRect2.xMax + spacing, rect.y, buttonWidth3, rect.height);

                string name = $"{package.Owner} - {package.Name.Replace("_", " ")} ({package.Versions[0].VersionNumber})";

                GUI.Label(modNameRect, name);

                string tempDownloadPath = Path.Combine(CoreModule.Instance.TempPath, "Thunderstore", package.FullName);
                string tempFilePath = Path.Combine(tempDownloadPath, $"{package.FullName}.zip");

                if(package.Versions[0].WebsiteUrl != ""
                    && package.Versions[0].WebsiteUrl.StartsWith("https://github.com/")
                    && package.Versions[0].WebsiteUrl != "https://github.com/thunderstore-io"
                    && GUI.Button(buttonRect1, "Import From Source")
                ) {
                    bool import = EditorUtility.DisplayDialog("Import Mod",
                        $"Are you sure you want to import '{name}'?", "Yes", "No");

                    if(import) {
                        using(GitHubClient client = new GitHubClient()) {
                            (string selectedOwner, string selectedRepo) = GithubUtils.ExtractOwnerAndRepo(package.Versions[0].WebsiteUrl);

                            if(!Directory.Exists(tempDownloadPath))
                                Directory.CreateDirectory(tempDownloadPath);

                            client.DownloadGithubZip(tempFilePath, selectedOwner, selectedRepo);

                            ZipFile.ExtractToDirectory(tempFilePath, tempDownloadPath);
                            string firstDirectoryPath = Directory.GetDirectories(tempDownloadPath)[0];

                            ModManagerModule.ConvertToUnityProject(firstDirectoryPath, selectedRepo);

                            Directory.Delete(tempDownloadPath, true);
                        }

                        LoggerUtils.Log($"Import from source: {package.FullName}");
                    }
                }

                if(GUI.Button(buttonRect2, "Import")) {
                    bool import = EditorUtility.DisplayDialog("Import Mod",
                        $"Are you sure you want to import '{name}'?", "Yes", "No");

                    if(import) {
                        if(!Directory.Exists(tempDownloadPath))
                            Directory.CreateDirectory(tempDownloadPath);

                        client.DownloadPackage(package, tempFilePath);

                        ZipFile.ExtractToDirectory(tempFilePath, tempDownloadPath);

                        string[] dlls = Directory.GetFiles(tempDownloadPath, "*.dll", SearchOption.AllDirectories);
                        foreach(string dll in dlls) {
                            File.Copy(dll, Path.Combine(CoreModule.Instance.DllsFolderPath, Path.GetFileName(dll)), true);

                            LoggerUtils.Log($"Imported: {Path.GetFileName(dll)}");
                        }

                        Directory.Delete(tempDownloadPath, true);

                        LoggerUtils.Log($"Import: {package.FullName}");
                    }
                }

                if(GUI.Button(buttonRect3, "View")) {
                    Application.OpenURL(package.PackageUrl);
                    LoggerUtils.Log($"View: {package.FullName}");
                }
            };
        }
        private void SetupThunderstoreClient() {
            if(client == null) {
                client = new ThunderstoreApiClient(TimeSpan.FromMinutes(5));
                AppDomain.CurrentDomain.DomainUnload += (sender, args) => {
                    client?.Dispose();
                };

                categories = client.GetCategories(COMMUNITY);
                selectedWhitelistedCategories = new bool[categories.Length];
                selectedBlacklistedCategories = new bool[categories.Length];
            }
        }
    }
}
