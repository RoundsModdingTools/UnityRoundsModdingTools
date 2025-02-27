using System;
using System.Linq;
using System.Threading.Tasks;
using ThunderstoreAPI;
using ThunderstoreAPI.Entities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using URMT.Core.Utils;

namespace Thunderstore.Windows {
    public enum PackageSortType {
        LastUpdated,
        Newest,
        MostDownloaded,
        TopRated,
    }

    internal class ThunderstoreWindow : EditorWindow {
        private const string COMMUNITY = "rounds";

        private static Package[] packages = new Package[0];
        private static Package[] shownPackages = new Package[0];
        private static string searchQuery = "";
        private PackageSortType sortType;

        private static ThunderstoreApiClient client;
        private static ReorderableList modList;
        private Vector2 scrollPos;


        [MenuItem("URMT/Thunderstore")]
        public static void ShowWindow() {
            GetWindow<ThunderstoreWindow>("Thunderstore");
        }

        public void Dispose() {
            client?.Dispose();
        }

        private async void OnEnable() {
            // Grab the packages on enable (async)
            packages = new Package[0];
            shownPackages = new Package[0];

            if(client == null) {
                client = new ThunderstoreApiClient(TimeSpan.FromMinutes(5));

                AppDomain.CurrentDomain.DomainUnload += (sender, args) => {
                    client?.Dispose();
                };

            }

            await GetPackages();
        }

        private void OnGUI() {
            EditorGUILayout.LabelField("Thunderstore", EditorStyles.boldLabel);
            if(modList == null) CreateModList();
            if(client == null) {
                client = new ThunderstoreApiClient(TimeSpan.FromMinutes(5));

                AppDomain.CurrentDomain.DomainUnload += (sender, args) => {
                    client?.Dispose();
                };

            }

            GUILayout.Label("Search:", GUILayout.Width(50));
            searchQuery = EditorGUILayout.TextField(searchQuery);
            sortType = (PackageSortType)EditorGUILayout.EnumPopup("Sort by:", sortType);


            if(packages.Length == 0) {
                EditorGUILayout.LabelField("Loading packages...");
                return;
            }

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

                // Add some padding at top/bottom
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                // Define button widths
                float spacing = 5f;
                float buttonWidth1 = 130f;
                float buttonWidth2 = 50f;
                float buttonWidth3 = 45f;
                float totalButtonWidth = buttonWidth1 + buttonWidth2 + buttonWidth3 + (spacing * 2);

                // Subtract buttons from total width
                float contentWidth = rect.width - totalButtonWidth - spacing;

                // Decide how much space ModName vs. Owner each get
                // e.g. 60% for ModName, 40% for Owner
                float modNameWidth = contentWidth * 0.6f;
                float ownerWidth = contentWidth * 0.4f;

                // Define Rects
                Rect modNameRect = new Rect(rect.x, rect.y, modNameWidth, rect.height);
                Rect ownerRect = new Rect(rect.x, rect.y, rect.xMax, rect.height);

                // Now define the button rects to the right
                Rect buttonRect1 = new Rect(rect.xMax - totalButtonWidth, rect.y, buttonWidth1, rect.height);
                Rect buttonRect2 = new Rect(buttonRect1.xMax + spacing, rect.y, buttonWidth2, rect.height);
                Rect buttonRect3 = new Rect(buttonRect2.xMax + spacing, rect.y, buttonWidth3, rect.height);

                // Create a centered style (or reuse EditorStyles.centeredGreyMiniLabel, etc.)
                var centeredStyle = new GUIStyle(GUI.skin.label) {
                    alignment = TextAnchor.MiddleCenter
                };

                // Draw labels
                GUI.Label(modNameRect, package.FullName);
                GUI.Label(ownerRect, package.Owner, centeredStyle);

                // Draw buttons
                if(GUI.Button(buttonRect1, "Import From Source")) {
                    // TODO: Implement the import from source logic
                    LoggerUtils.Log($"Import from source: {package.FullName}");
                }

                if(GUI.Button(buttonRect2, "Import")) {
                    // TODO: Implement the import logic
                    LoggerUtils.Log($"Import: {package.FullName}");
                }

                if(GUI.Button(buttonRect3, "View")) {
                    Application.OpenURL(package.PackageUrl);
                    LoggerUtils.Log($"View: {package.FullName}");
                }
            };
        }
    }
}
