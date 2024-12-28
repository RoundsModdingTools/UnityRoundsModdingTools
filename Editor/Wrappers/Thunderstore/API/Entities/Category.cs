using Newtonsoft.Json;

namespace UnityRoundsModdingTools.Editor.Thunderstore.API.Entities {
    public struct Category {
        [JsonProperty("name")] public string Name;
        [JsonProperty("slug")] public string Slug;
    }
}
