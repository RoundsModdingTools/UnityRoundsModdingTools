using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderstoreAPI.Entities.UserMedia {
    public struct UploadPartUrl {
        [JsonProperty("part_number")] public int PartNumber;
        [JsonProperty("url")] public Uri Url;
        [JsonProperty("offset")] public int Offset;
        [JsonProperty("length")] public int Length;
    }
}
