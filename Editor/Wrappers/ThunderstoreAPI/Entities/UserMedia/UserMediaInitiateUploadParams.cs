using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderstoreAPI.Entities.UserMedia {
    public struct UserMediaInitiateUploadParams {
        [JsonProperty("filename")] public string Filename;
        [JsonProperty("file_size_bytes")] public int FileSizeBytes;
    }
}
