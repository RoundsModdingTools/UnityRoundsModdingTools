using Newtonsoft.Json;

namespace ThunderstoreAPI.Entities {
    public struct ErrorResponse {
        [JsonProperty("file")] public string[] File { get; set; }
    }
}
