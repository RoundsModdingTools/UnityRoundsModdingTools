using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ThunderstoreAPI.Entities {
    public struct PackageSubmissionMetadata {
        [JsonProperty("author_name")] public string AuthorName { get; set; }

        [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)] public string[] Categories { get; set; }

        [JsonProperty("communities")] public string[] Communities { get; set; }

        [JsonProperty("has_nsfw_content")] public bool HasNSFWContent { get; set; }

        [JsonProperty("upload_uuid")] public Guid UploadUUID { get; set; }

        [JsonProperty("community_categories", NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, string[]> CommunityCategories { get; set; }

        public PackageSubmissionMetadata(string authorName, string[] categories, string[] communities, bool hasNSFWContent, Guid uploadUUID, Dictionary<string, string[]> communityCategories) {
            AuthorName = authorName;
            Categories = categories;
            Communities = communities ?? new string[0];
            HasNSFWContent = hasNSFWContent;
            UploadUUID = uploadUUID;
            CommunityCategories = communityCategories;
        }
    }
}
