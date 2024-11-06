using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using UnityRoundsModdingTools.Editor.Thunderstore.API.Entities;

namespace UnityRoundsModdingTools.Editor.Thunderstore.API {
    public class ThunderstoreAPI {
        public List<Package> GetPackages(string community) {
            UnityEngine.Debug.Log("Fetching packages from Thunderstore...");

            using(var client = new HttpClient()) {
                var url = $"https://thunderstore.io/c/{community}/api/v1/package/";
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                List<Package> packages = JsonConvert.DeserializeObject<List<Package>>(content);

                return packages;
            }
        }
    }
}
