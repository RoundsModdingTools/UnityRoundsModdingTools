using Newtonsoft.Json;

namespace ThunderstoreAPI.Entities {
    public struct Category {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("slug")] public string Slug { get; set; }
    }
}
