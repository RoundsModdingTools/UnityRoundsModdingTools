using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderstoreAPI.Entities.UserMedia {
    public struct UserMediaInitiateUploadResponse {
        [JsonProperty("user_media")] public UserMedia UserMedia;
        [JsonProperty("upload_urls")] public UploadPartUrl[] UploadUrls;
    }
}
