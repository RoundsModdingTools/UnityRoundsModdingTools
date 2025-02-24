using System.Text.RegularExpressions;

namespace GitHubAPI {
    public static class GithubUtils {
        public static bool IsValidGithubUrl(string url) {
            string pattern = @"^https://github\.com/([^/]+/[^/]+)$";
            return Regex.IsMatch(url, pattern);
        }

        public static (string owner, string repo) ExtractOwnerAndRepo(string githubUrl) {
            string[] repoParts = githubUrl.Replace("https://github.com/", "").Split('/');
            return (repoParts[0], repoParts[1]);
        }
    }
}
