using Newtonsoft.Json;
using System;

namespace ThunderstoreAPI.Entities.UserMedia {
    public struct UserMedia {
        [JsonProperty("uuid")] public Guid UUID { get; set; }
        [JsonProperty("filename")] public string Filename { get; set; }
        [JsonProperty("size")] public long Size { get; set; }
        [JsonProperty("datetime_created")] public DateTime DateTimeCreated { get; set; }
        [JsonProperty("expiry")] public DateTime Expiry { get; set; }
        [JsonProperty("status")] private string status { get; set; }

        [JsonIgnore]
        public Status Status {
            get {
                return (Status)Enum.Parse(typeof(Status), status);
            }
        }
    }
}
