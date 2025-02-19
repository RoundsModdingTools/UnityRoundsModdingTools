using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderstoreAPI.Entities {
    public struct Category {
        [JsonProperty("name")] public string Name;
        [JsonProperty("slug")] public string Slug;
    }
}
