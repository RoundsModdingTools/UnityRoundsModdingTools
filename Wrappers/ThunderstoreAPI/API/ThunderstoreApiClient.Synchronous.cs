using System;
using ThunderstoreAPI.Entities;

namespace ThunderstoreAPI {
    public partial class ThunderstoreApiClient {
        public Package[] GetPackages(string community) {
            if(cachedPackages.TryGetValue(community, out (Package[], DateTimeOffset) cachedPackagesEntry)) {
                if(DateTimeOffset.Now - cachedPackagesEntry.Item2 < cacheDuration) {
                    return cachedPackagesEntry.Item1;
                }
            }

            return GetPackagesAsync(community).Result;
        }

        public Category[] GetCategories() {
            if(cachedCategories.TryGetValue("categories", out Category[] cachedCategoriesEntry)) {
                return cachedCategoriesEntry;
            }

            return GetCategoriesAsync().Result;
        }

        public void Publish(PublishOption publishOption, Byte[] data, string token) {
            PublishAsync(publishOption, data, token).Wait();
        }
    }
}
