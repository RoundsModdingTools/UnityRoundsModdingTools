using GitHubAPI.Entities;
using System.Threading.Tasks;

namespace GitHubAPI {
    public partial class GitHubClient {
        public void DownloadGithubZip(string savePath, string owner, string repo, string branch = "") {
            Task.Run(() => DownloadGithubZipAsync(savePath, owner, repo, branch)).Wait();
        }

        public void DownloadReleaseZip(string savePath, string owner, string repo, string tag = "latest") {
            Task.Run(() => DownloadReleaseZipAsync(savePath, owner, repo, tag)).Wait();
        }

        public void DownloadReleaseAsset(string savePath, string owner, string repo, string assetName, string tag = "latest") {
            Task.Run(() => DownloadReleaseAssetAsync(savePath, owner, repo, assetName, tag)).Wait();
        }

        public void DownloadReleaseAssetWithFileExtension(string savePath, string owner, string repo, string fileExtension, string tag = "latest") {
            Task.Run(() => DownloadReleaseAssetWithFileExtensionAsync(savePath, owner, repo, fileExtension, tag)).Wait();
        }

        public GitHubRelease GetLatestReleaseTag(string owner, string repo) {
            return Task.Run(() => GetLatestReleaseTagAsync(owner, repo)).Result;
        }

        public GitHubRelease GetReleaseByTag(string owner, string repo, string tag) {
            return Task.Run(() => GetReleaseByTagAsync(owner, repo, tag)).Result;
        }

        public GitHubRelease[] GetReleases(string owner, string repo) {
            return Task.Run(() => GetReleasesAsync(owner, repo)).Result;
        }

        public GithubContent GetContentFromPath(string owner, string repo, string path) {
            return Task.Run(() => GetContentFromPathAsync(owner, repo, path)).Result;
        }
    }
}
