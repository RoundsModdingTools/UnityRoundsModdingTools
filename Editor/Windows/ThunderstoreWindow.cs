﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityRoundsModdingTools.ScriptableObjects;
using UnityRoundsModdingTools.Thunderstore.API;
using UnityRoundsModdingTools.Thunderstore.API.Entities;
using UnityRoundsModdingTools.Utils;

namespace UnityRoundsModdingTools.Windows {
    public class ThunderstoreWindow : EditorWindow {
        private static ThunderstoreAPI thunderstoreAPI = new ThunderstoreAPI();
        private static List<Package> packages = new List<Package>();

        private string searchQuery = "";
        private string previousSearchQuery = "";

        private PackageSortType sortType;
        private PackageSortType previousSortType;

        [MenuItem("Unity Rounds Modding Tools/Thunderstore")]
        private static void ShowWindow() {
            GetWindow(typeof(ThunderstoreWindow), false, "Thunderstore");
        }

        private void OnEnable() {
            searchQuery = EditorPrefs.GetString("ThunderstoreSearchQuery");
            sortType = (PackageSortType)EditorPrefs.GetInt("ThunderstoreSortType", (int)PackageSortType.MostDownloaded);
            previousSearchQuery = searchQuery;
            previousSortType = sortType;

            if(packages.Count == 0) {
                packages = thunderstoreAPI.SearchPackage(searchQuery, sortType, "rounds");
                packages.RemoveAll(package => package.FullName == "ebkr-r2modman");
                packages = packages.Take(50).ToList();
            }
        }

        private bool isSearchQueryDifferent() {
            return previousSearchQuery != searchQuery;
        }

        private void OnGUI() {
            EditorPrefs.SetString("ThunderstoreSearchQuery", searchQuery);
            EditorPrefs.SetInt("ThunderstoreSortType", (int)sortType);

            GUILayout.Space(10);

            GUIStyle headerLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            headerLabelStyle.alignment = TextAnchor.MiddleCenter;
            headerLabelStyle.fontSize = 18;

            GUILayout.Label("Thunderstore", headerLabelStyle);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            searchQuery = EditorGUILayout.TextField(searchQuery);
            if((GUILayout.Button("Search", GUILayout.Width(60)) || Event.current.keyCode == KeyCode.Return) &&
                (previousSearchQuery != searchQuery || previousSortType != sortType)) {

                packages = thunderstoreAPI.SearchPackage(searchQuery, sortType, "rounds");
                packages.RemoveAll(package => package.FullName == "ebkr-r2modman");
                packages = packages.Take(50).ToList();

                previousSearchQuery = searchQuery;
                previousSortType = sortType;
            }
            GUILayout.EndHorizontal();
            sortType = (PackageSortType)EditorGUILayout.EnumPopup("Sort by:", sortType);

            GUILayout.Space(10);

            if(packages.Count == 0) {
                EditorGUILayout.HelpBox("No packages found.", MessageType.Info);
            } else {
                GUIUtils.DrawListEntries(1, ref packages, (package) => {
                    GUILayout.Label(package.Name, EditorStyles.boldLabel);

                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Owner: {package.Owner}");
                    if(package.Versions[0].WebsiteUrl != ""
                    && package.Versions[0].WebsiteUrl.StartsWith("https://github.com/")
                    && package.Versions[0].WebsiteUrl != "https://github.com/thunderstore-io") {
                        if(GUILayout.Button("Install", GUILayout.MaxWidth(60), GUILayout.MaxHeight(15))) {
                            bool result = EditorUtility.DisplayDialog("Install Mod", $"Are you sure you want to install \"{package.Name}\"?", "Yes", "Cancel");
                            if(result) {
                                var installAssemblies = GithubUtils.DownloadGithubProject(package.Versions[0].WebsiteUrl);
                                if(installAssemblies != null) {
                                    foreach(string installAssembly in installAssemblies) {
                                        ProjectMappings.Instance.folderMappings.Add(new FolderMapping(installAssembly, "Libraries"));
                                    }
                                    ProjectMappings.Save();
                                    AssetDatabase.Refresh();
                                }
                            }
                        }
                    }

                    if(GUILayout.Button("Open", GUILayout.MaxWidth(50), GUILayout.MaxHeight(15))) {
                        Application.OpenURL($"https://thunderstore.io/c/rounds/p/{package.Owner}/{package.Name}/");
                    }
                });
            }
        }
    }
}
