using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ThunderstoreAPI.Entities {
    public struct PackageSubmissionMetadata {
        [JsonProperty("author_name")]
        public string AuthorName;

        [JsonProperty("categories")]
        public List<string> Categories;

        [JsonProperty("communities")]
        public List<string> Communities;

        [JsonProperty("has_nsfw_content")]
        public bool HasNSFWContent;

        [JsonProperty("upload_uuid")]
        public Guid UploadUUID;

        [JsonProperty("community_categories")]
        public Dictionary<string, List<string>> CommunityCategories;

        public PackageSubmissionMetadata(string authorName, List<string> categories, List<string> communities, bool hasNSFWContent, Guid uploadUUID, Dictionary<string, List<string>> communityCategories) {
            AuthorName = authorName;
            Categories = categories ?? new List<string>();
            Communities = communities ?? new List<string>();
            HasNSFWContent = hasNSFWContent;
            UploadUUID = uploadUUID;
            CommunityCategories = communityCategories ?? new Dictionary<string, List<string>>();
        }
    }
}
