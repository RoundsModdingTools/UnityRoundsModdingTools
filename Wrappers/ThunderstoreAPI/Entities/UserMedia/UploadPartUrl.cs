using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderstoreAPI.Entities.UserMedia {
    public struct UploadPartUrl {
        [JsonProperty("part_number")] public int PartNumber { get; set; }
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("offset")] public int Offset { get; set; }
        [JsonProperty("length")] public int Length { get; set; }
    }
}
