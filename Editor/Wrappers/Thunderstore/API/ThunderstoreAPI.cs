using Newtonsoft.Json;
using System;
using System.Net.Http;
using UnityRoundsModdingTools.Editor.Thunderstore.API.Entities;

namespace UnityRoundsModdingTools.Editor.Thunderstore.API {
    public class ThunderstoreAPI {
        public TimeSpan cacheDuration = TimeSpan.FromMinutes(5);

        private Package[] cachedPackages;
        private DateTimeOffset lastFetchTime;

        public Package[] GetPackages(string community) {
            UnityEngine.Debug.Log("Fetching packages from Thunderstore...");

            using(var client = new HttpClient()) {
                if(cachedPackages != null && DateTimeOffset.Now - lastFetchTime < cacheDuration) {
                    return cachedPackages;
                }

                var url = $"https://thunderstore.io/c/{community}/api/v1/package/";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                Package[] packages = JsonConvert.DeserializeObject<Package[]>(content);
                cachedPackages = packages;
                lastFetchTime = DateTimeOffset.Now;

                return packages;
            }
        }
    }
}
