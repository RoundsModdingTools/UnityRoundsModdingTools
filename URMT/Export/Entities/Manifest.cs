using Newtonsoft.Json;

namespace URMT.Export.Entities {
    public struct Manifest {
        [JsonProperty("name")] public string ModName;
        [JsonProperty("version_number")] public string Version;
        [JsonProperty("website_url")] public string WebsiteURL;
        [JsonProperty("description")] public string Description;
        [JsonProperty("dependencies")] public string[] Dependencies;
    }
}
