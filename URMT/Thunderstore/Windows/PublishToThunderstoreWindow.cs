using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThunderstoreAPI;
using ThunderstoreAPI.Entities;
using UnityEditor;
using UnityEngine;
using URMT.Core.UI;
using URMT.Core.Utils;
using URMT.Export.ScriptableObjects;

namespace URMT.Thunderstore.Windows {
    public class PublishToThunderstoreWindow : EditorWindow {
        private const string COMMUNITY = "rounds";

        private static Regex modNameValidation = new Regex(@"^[a-zA-Z0-9_]+$");

        private static Category[] categories = new Category[0];
        private static bool[] selectedCategories = new bool[0];
        private static bool NSFWContent = false;

        private static ModInfo modInfo;
        private static ThunderstoreApiClient client;

        private static string ErrorMessage;

        private static bool isPublishing = false;
        private static bool isPublishSuccessful = false;

        [MenuItem("URMT/Thunderstore/Publish to Thunderstore")]
        public static void ShowWindow() {
            GetWindow<PublishToThunderstoreWindow>("Publish to Thunderstore");
        }

        private async void OnEnable() {
            SetupThunderstoreClient();
            await GetCategories();

            isPublishSuccessful = false;
            ErrorMessage = null;
        }

        private void OnGUI() {
            SetupThunderstoreClient();

            GUIUtils.DrawTitle("Publish to Thunderstore");

            if(categories.Length == 0) {
                EditorGUILayout.HelpBox("Loading categories...", MessageType.Info);
                return;
            }

            modInfo = EditorGUILayout.ObjectField("Mod Info", modInfo, typeof(ModInfo), false) as ModInfo;
            if(modInfo == null) {
                EditorGUILayout.HelpBox("Please select a ModInfo asset", MessageType.Info);
            } else {
                GUIUtils.CreateMultSelectDropdown("Categories", "Select Categories", categories.Select(x => x.Name).ToList(), selectedCategories);
                NSFWContent = EditorGUILayout.Toggle("NSFW Content", NSFWContent);

                // If there no categories selected, Set it to null
                string[] selectedCategoriesNames = selectedCategories.Any(x => x)
                    ? categories
                        .Where((x, i) => selectedCategories[i])
                        .Select(x => x.Name)
                        .ToArray()
                    : null;

                PublishOption publishOption = new PublishOption(modInfo.Author, selectedCategoriesNames, NSFWContent, COMMUNITY);


                if(!ValidateModInfo()) {
                    GUI.enabled = false;
                } else if(isPublishing) {
                    EditorGUILayout.HelpBox("Publishing...", MessageType.Info);
                    GUI.enabled = false;
                }

                if(GUILayout.Button("Publish")) {
                    isPublishSuccessful = false;

                    Task.Run(async () => {
                        isPublishing = true;
                        await PublishMod(publishOption);
                        Repaint();
                    });
                }

                GUI.enabled = true;

                if(!string.IsNullOrEmpty(ErrorMessage)) {
                    EditorGUILayout.HelpBox(ErrorMessage, MessageType.Error);
                } else if(isPublishSuccessful) {
                    EditorGUILayout.HelpBox("Mod published successfully", MessageType.Info);

                    if(GUILayout.Button("Open Thunderstore Page")) {
                        Application.OpenURL($"https://thunderstore.io/c/{COMMUNITY}/p/{modInfo.Author}/{modInfo.ModName}/");
                    }
                }
            }
        }

        public async Task PublishMod(PublishOption publishOption) {
            string exportPath = await MainThreadAction.InvokeAsync(() => modInfo.ExportMod());
            try {
                await client.PublishAsync(publishOption, $"{exportPath}.zip", ThunderstoreModuleSettings.ThunderstoreAPITokenRegistry);
                isPublishSuccessful = true;
                ErrorMessage = null;
            } catch(Exception e) {
                ErrorMessage = e.Message;
            }
            
            isPublishing = false;
        }

        private bool ValidateModInfo() {
            if(string.IsNullOrEmpty(modInfo.Author)) {
                EditorGUILayout.HelpBox("Please fill in the Author field in the ModInfo asset", MessageType.Error);
                return false;
            } else if(string.IsNullOrEmpty(modInfo.ModName)) {
                EditorGUILayout.HelpBox("Please fill in the ModName field in the ModInfo asset", MessageType.Error);
                return false;
            } else if(!modNameValidation.IsMatch(modInfo.ModName)) {
                EditorGUILayout.HelpBox("ModName can only contain letters, numbers, and underscores", MessageType.Error);
                return false;
            } else if(modInfo.Description.Length > 250) {
                EditorGUILayout.HelpBox("Description is too long, it must be less than 250 characters", MessageType.Error);
                return false;
            } else if(!modInfo.HasReadme) {
                EditorGUILayout.HelpBox("Please include a README.md file in the mod's root folder", MessageType.Error);
                return false;
            } else if(modInfo.Icon == null || modInfo.Icon.width != 256 || modInfo.Icon.height != 256) {
                EditorGUILayout.HelpBox("Please include a 256x256 icon in the mod's root folder", MessageType.Error);
                return false;
            } else if(modInfo.Version.Split('.').Length != 3) {
                EditorGUILayout.HelpBox("Version must be in the format of x.x.x", MessageType.Error);
                return false;
            } else if(string.IsNullOrEmpty(ThunderstoreModuleSettings.ThunderstoreAPITokenRegistry)) {
                EditorGUILayout.HelpBox("Please set your Thunderstore API token in the settings", MessageType.Error);
                return false;
            }

            return true;
        }

        private async Task GetCategories() {
            categories = await client.GetCategoriesAsync(COMMUNITY);
            selectedCategories = new bool[categories.Length];
        }

        private void SetupThunderstoreClient() {
            if(client == null) {
                client = new ThunderstoreApiClient(TimeSpan.FromMinutes(5));
                AppDomain.CurrentDomain.DomainUnload += (sender, args) => {
                    client?.Dispose();
                };

                categories = client.GetCategories(COMMUNITY);
                selectedCategories = new bool[categories.Length];
            }
        }
    }
}