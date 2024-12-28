using Newtonsoft.Json;
using System;
using System.Net.Http;
using UnityRoundsModdingTools.Editor.Thunderstore.API.Entities;

namespace UnityRoundsModdingTools.Editor.Thunderstore.API {


    public class ThunderstoreAPI {
        private const string THUNDERSTORE_API_URL = "https://thunderstore.io";
        
        private struct CategoryResponse {
            public CategoryResult results;
        }

        private struct CategoryResult {
            public Category[] categories;
        }



        public TimeSpan cacheDuration = TimeSpan.FromMinutes(5);

        private Package[] cachedPackages;
        private DateTimeOffset lastFetchTime;

        public Package[] GetPackages(string community) {
            UnityEngine.Debug.Log("Fetching packages from Thunderstore...");

            using(var client = new HttpClient()) {
                if(cachedPackages != null && DateTimeOffset.Now - lastFetchTime < cacheDuration) {
                    return cachedPackages;
                }

                var url = $"{THUNDERSTORE_API_URL}/c/{community}/api/v1/package/";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                Package[] packages = JsonConvert.DeserializeObject<Package[]>(content);
                cachedPackages = packages;
                lastFetchTime = DateTimeOffset.Now;

                return packages;
            }
        }

        public Category[] GetCategories(string community) {
            UnityEngine.Debug.Log("Fetching categories from Thunderstore...");
            using(var client = new HttpClient()) {
                var url = $"{THUNDERSTORE_API_URL}/api/experimental/{community}/rounds/category/";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                CategoryResponse categoryResponse = JsonConvert.DeserializeObject<CategoryResponse>(content);
                return categoryResponse.results.categories;
            }
        }
    }
}
