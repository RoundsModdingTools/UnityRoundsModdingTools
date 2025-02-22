using Newtonsoft.Json;

namespace ThunderstoreAPI.Entities.UserMedia {
    public struct UserMediaInitiateUploadResponse {
        [JsonProperty("user_media")] public UserMedia UserMedia { get; set; }
        [JsonProperty("upload_urls")] public UploadPartUrl[] UploadUrls { get; set; }
    }
}