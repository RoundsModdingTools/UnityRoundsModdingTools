using System;
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
        public static Package[] SearchPackage(this ThunderstoreAPI api, string searchTerm, PackageSortType sortType, string community) {
            Package[] packages = api.GetPackages(community);

            IEnumerable<Package> filteredPackages =
                from package in packages
                where package.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                   || package.FullName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                select package;


            filteredPackages = filteredPackages.SortPackages(sortType);

            return filteredPackages.ToArray();
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
