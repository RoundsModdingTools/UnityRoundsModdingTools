using Newtonsoft.Json;
using System;

namespace UnityRoundsModdingTools.Thunderstore.API.Entities {
    public struct Package {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("full_name")] public string FullName { get; set; }
        [JsonProperty("owner")] public string Owner { get; set; }
        [JsonProperty("package_url")] public string PackageUrl { get; set; }
        [JsonProperty("donation_link")] public string DonationLink { get; set; }
        [JsonProperty("date_created")] public DateTime DateCreated { get; set; }
        [JsonProperty("date_updated")] public DateTime DateUpdated { get; set; }
        [JsonProperty("uuid4")] public Guid Uuid4 { get; set; }
        [JsonProperty("rating_score")] public int RatingScore { get; set; }
        [JsonProperty("is_pinned")] public bool IsPinned { get; set; }
        [JsonProperty("is_deprecated")] public bool IsDeprecated { get; set; }
        [JsonProperty("has_nsfw_content")] public bool HasNsfwContent { get; set; }
        [JsonProperty("categories")] public string[] Categories { get; set; }
        [JsonProperty("versions")] public PackageVersion[] Versions { get; set; }
    }
}
