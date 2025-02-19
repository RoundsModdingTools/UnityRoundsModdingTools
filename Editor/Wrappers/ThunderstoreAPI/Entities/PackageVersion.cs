using Newtonsoft.Json;
using System;

namespace ThunderstoreAPI.Entities {
    public struct PackageVersion {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("full_name")] public string FullName { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("version_number")] public string VersionNumber { get; set; }
        [JsonProperty("dependencies")] public string[] Dependencies { get; set; }
        [JsonProperty("download_url")] public string DownloadUrl { get; set; }
        [JsonProperty("downloads")] public int Downloads { get; set; }
        [JsonProperty("date_created")] public DateTime DateCreated { get; set; }
        [JsonProperty("website_url")] public string WebsiteUrl { get; set; }
        [JsonProperty("is_active")] public bool IsActive { get; set; }
        [JsonProperty("uuid")] public string Uuid { get; set; }
        [JsonProperty("file_size")] public long FileSize { get; set; }
    }
}
