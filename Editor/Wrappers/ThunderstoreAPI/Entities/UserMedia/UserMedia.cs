using Newtonsoft.Json;
using System;
using ThunderstoreAPI.Entities.UserMedia;

namespace ThunderstoreAPI.Entities.UserMedia {
    public struct UserMedia {
        [JsonProperty("uuid")] public string UUID;
        [JsonProperty("filename")] public string Filename;
        [JsonProperty("size")] public long Size;
        [JsonProperty("datetime_created")] public DateTime DateTimeCreated;
        [JsonProperty("expiry")] public DateTime Expiry;
        [JsonProperty("status")] private string status;
        [JsonIgnore]
        public Status Status {
            get {
                return (Status)Enum.Parse(typeof(Status), status);
            }
        }
    }
}
