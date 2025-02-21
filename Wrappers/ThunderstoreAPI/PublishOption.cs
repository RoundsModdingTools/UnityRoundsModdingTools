using System.Collections.Generic;

namespace ThunderstoreAPI {
    public struct PublishOption {
        public string AuthorName;
        public string[] Categories;
        public string[] Communities;
        public bool HasNSFWContent;
        public Dictionary<string, string[]> CommunityCategories;

        public PublishOption(string authorName, string[] categories, bool hasNSFWContent, string[] communities, Dictionary<string, string[]> communityCategories) {
            AuthorName = authorName;
            Categories = categories ?? new string[0];
            Communities = communities ?? new string[0];
            HasNSFWContent = hasNSFWContent;
            CommunityCategories = communityCategories ?? new Dictionary<string, string[]>();
        }

        public PublishOption(string authorName, string[] categories, bool hasNSFWContent, string communities, string[] communityCategories) {
            AuthorName = authorName;
            Categories = categories ?? new string[0];
            Communities = new string[] { communities };
            HasNSFWContent = hasNSFWContent;
            CommunityCategories = new Dictionary<string, string[]> { { communities, communityCategories } };
        }
    }
}
