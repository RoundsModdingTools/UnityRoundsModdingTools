using System.Collections.Generic;
using System.Linq;
using UnityRoundsModdingTools.Editor.Thunderstore.API.Entities;

namespace UnityRoundsModdingTools.Editor.Thunderstore.API {
    public enum PackageSortType {
        LastUpdated,
        Newest,
        MostDownloaded,
        TopRated,
    }

    public static class ThunderstoreAPIExtension {
        public static List<Package> SearchPackage(this ThunderstoreAPI api, string searchTerm, PackageSortType sortType, string community) {
            List<Package> packages = api.GetPackages(community);

            IEnumerable<Package> filteredPackages =
                from package in packages
                where package.Name.Contains(searchTerm) || package.FullName.Contains(searchTerm)
                select package;

            filteredPackages = filteredPackages.SortPackages(sortType);

            return filteredPackages.ToList();
        }

        private static IEnumerable<Package> SortPackages(this IEnumerable<Package> packages, PackageSortType sortType) {
            switch(sortType) {
                case PackageSortType.LastUpdated:
                    return packages.OrderByDescending(package => package.DateUpdated);
                case PackageSortType.Newest:
                    return packages.OrderByDescending(package => package.DateCreated);
                case PackageSortType.MostDownloaded:
                    return packages.OrderByDescending(package => package.Versions.Sum(version => version.Downloads));
                case PackageSortType.TopRated:
                    return packages.OrderByDescending(package => package.RatingScore);
                default:
                    return packages;
            }
        }
    }
}
