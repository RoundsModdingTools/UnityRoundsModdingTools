using Newtonsoft.Json;

namespace ThunderstoreAPI.Entities {
    public struct CategoryResponse {
        [JsonProperty("results")] public Category[] Categories { get; set; }
    }
}
