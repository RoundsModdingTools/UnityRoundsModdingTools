using Newtonsoft.Json;
using System;

namespace GitHubAPI.Entities {
    public struct GithubContent {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("path")] public string Path { get; set; }
        [JsonProperty("sha")] public string Sha { get; set; }
        [JsonProperty("size")] public int Size { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
        [JsonProperty("html_url")] public string HtmlUrl { get; set; }
        [JsonProperty("git_url")] public string GitUrl { get; set; }
        [JsonProperty("download_url")] public string DownloadUrl { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
        [JsonProperty("encoding")] public string Encoding { get; set; }

        [JsonIgnore] public byte[] DecodedContent => Convert.FromBase64String(Content);
        [JsonIgnore] public string DecodedContentString => System.Text.Encoding.UTF8.GetString(DecodedContent);
    }
}
