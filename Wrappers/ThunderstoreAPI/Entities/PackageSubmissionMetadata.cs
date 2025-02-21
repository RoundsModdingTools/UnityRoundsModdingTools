using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ThunderstoreAPI.Entities {
    public struct PackageSubmissionMetadata {
        [JsonProperty("author_name")]
        public string AuthorName;

        [JsonProperty("categories")]
        public string[] Categories;

        [JsonProperty("communities")]
        public string[] Communities;

        [JsonProperty("has_nsfw_content")]
        public bool HasNSFWContent;

        [JsonProperty("upload_uuid")]
        public Guid UploadUUID;

        [JsonProperty("community_categories")]
        public Dictionary<string, string[]> CommunityCategories;

        public PackageSubmissionMetadata(string authorName, string[] categories, string[] communities, bool hasNSFWContent, Guid uploadUUID, Dictionary<string, string[]> communityCategories) {
            AuthorName = authorName;
            Categories = categories ?? new string[0];
            Communities = communities ?? new string[0];
            HasNSFWContent = hasNSFWContent;
            UploadUUID = uploadUUID;
            CommunityCategories = communityCategories ?? new Dictionary<string, string[]>();
        }
    }
}
