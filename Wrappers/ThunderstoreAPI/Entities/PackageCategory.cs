using Newtonsoft.Json;

namespace ThunderstoreAPI.Entities {
    public struct Category {
        [JsonProperty("name")] public string Name;
        [JsonProperty("slug")] public string Slug;
    }
}
