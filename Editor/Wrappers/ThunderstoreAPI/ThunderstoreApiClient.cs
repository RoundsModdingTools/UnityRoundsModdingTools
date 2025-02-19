using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using ThunderstoreAPI.Entities;

namespace ThunderstoreAPI {
    public partial class ThunderstoreApiClient {
        private const string THUNDERSTORE_API_URL = "https://thunderstore.io";

        Dictionary<string, (Package[], DateTimeOffset)> cachedPackages = new Dictionary<string, (Package[], DateTimeOffset)>();
        Dictionary<string, Category[]> cachedCategories = new Dictionary<string, Category[]>();

        private TimeSpan cacheDuration;

        public ThunderstoreApiClient(TimeSpan cacheDuration) {
            this.cacheDuration = cacheDuration;
        }

        public Package[] GetPackages(string community) {
            using(var client = new HttpClient()) {
                if(cachedPackages.TryGetValue(community, out (Package[], DateTimeOffset) cachedPackagesEntry)) {
                    if(DateTimeOffset.Now - cachedPackagesEntry.Item2 < cacheDuration) {
                        return cachedPackagesEntry.Item1;
                    }
                }

                var url = $"{THUNDERSTORE_API_URL}/c/{community}/api/v1/package/";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                Package[] packages = JsonConvert.DeserializeObject<Package[]>(content);
                cachedPackages[community] = (packages, DateTimeOffset.Now);

                return packages;
            }
        }

        public Category[] GetCategories() {
            using(var client = new HttpClient()) {
                if(cachedCategories.TryGetValue("categories", out Category[] cachedCategoriesEntry)) {
                    return cachedCategoriesEntry;
                }

                var url = $"{THUNDERSTORE_API_URL}/api/v1/category/";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                Category[] categories = JsonConvert.DeserializeObject<Category[]>(content);
                cachedCategories["categories"] = categories;
                return categories;
            }
        }

        public void InitUpload(string filename, int fizeSizeBytes, string token) {
            using(var client = new HttpClient()) {
                var url = $"{THUNDERSTORE_API_URL}/api/v1/package/upload/init/";
                var content = new StringContent(JsonConvert.SerializeObject(new {
                    filename,
                    fizeSizeBytes,
                }), System.Text.Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content).Result;
                // Set the bearer token
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
