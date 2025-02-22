using Newtonsoft.Json;
using System;
using System.Linq;

namespace GitHubAPI.Entities {
    public struct GitHubAsset {
        [JsonProperty("url")] public string Url;
        [JsonProperty("id")] public long Id;
        [JsonProperty("node_id")] public string NodeId;
        [JsonProperty("name")] public string Name;
        [JsonProperty("label")] public string Label;
        [JsonProperty("uploader")] public GitHubUser Uploader;
        [JsonProperty("content_type")] public string ContentType;
        [JsonProperty("state")] public string State;
        [JsonProperty("size")] public long Size;
        [JsonProperty("download_count")] public int DownloadCount;
        [JsonProperty("created_at")] public DateTime CreatedAt;
        [JsonProperty("updated_at")] public DateTime UpdatedAt;
        [JsonProperty("browser_download_url")] public string BrowserDownloadUrl;

        [JsonIgnore] public string FileName => BrowserDownloadUrl.Split('/').Last();
        [JsonIgnore] public string FileExtension => FileName.Split('.').Last();
    }
}
