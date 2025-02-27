using System;
using System.Threading.Tasks;
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

        public void DownloadPackage(Package package, string downloadPath, string targetVersion = null) {
            Task.Run(() => DownloadPackageAsync(package, downloadPath, targetVersion)).Wait();
        }

        public Category[] GetCategories(string community) {
            if(cachedCategories.TryGetValue(community, out Category[] cachedCategoriesEntry)) {
                return cachedCategoriesEntry;
            }

            return GetCategoriesAsync(community).Result;
        }

        public void Publish(PublishOption publishOption, Byte[] data, string token) {
            Task.Run(() => PublishAsync(publishOption, data, token)).Wait();
        }
    }
}
