using GitHubAPI.Entities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GitHubAPI {
    public partial class GitHubClient : IDisposable {
        private const string API_URL = "https://api.github.com";

        private readonly WebClient webClient;
        private readonly HttpClient httpClient;

        public GitHubClient() {
            webClient = new WebClient();
            httpClient = new HttpClient();

            webClient.Headers.Add("User-Agent", "UnityRoundsModdingTools");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "UnityRoundsModdingTools");
        }

        private void EnsurePathExists(string path) {
            if(System.IO.Directory.Exists(path)) {
                System.IO.Directory.Delete(path, true);
            }
            System.IO.Directory.CreateDirectory(path);
        }

        public async Task DownloadGithubZipAsync(string savePath, string owner, string repo, string branch = "") {
            EnsurePathExists(System.IO.Path.GetDirectoryName(savePath));

            await webClient.DownloadFileTaskAsync($"{API_URL}/repos/{owner}/{repo}/zipball/{branch}", savePath);
        }

        public async Task DownloadReleaseZipAsync(string savePath, string owner, string repo, string tag = "latest") {
            EnsurePathExists(System.IO.Path.GetDirectoryName(savePath));

            var release = tag == "latest"
                ? await GetLatestReleaseTagAsync(owner, repo)
                : await GetReleaseByTagAsync(owner, repo, tag);

            await webClient.DownloadFileTaskAsync(release.ZipballUrl, savePath);
        }

        public async Task DownloadReleaseAssetAsync(string savePath, string owner, string repo, string assetName, string tag = "latest") {
            EnsurePathExists(System.IO.Path.GetDirectoryName(savePath));

            var release = await GetReleaseByTagAsync(owner, repo, tag);
            var asset = release.Assets
                .Where(a => a.Name == assetName)
                .FirstOrDefault();

            await webClient.DownloadFileTaskAsync(asset.BrowserDownloadUrl, savePath);
        }

        public async Task DownloadReleaseAssetWithFileExtensionAsync(string savePath, string owner, string repo, string fileExtension, string tag = "latest") {
            EnsurePathExists(System.IO.Path.GetDirectoryName(savePath));

            var release = await GetReleaseByTagAsync(owner, repo, tag);
            var asset = release.Assets
                .Where(a => a.FileExtension == fileExtension)
                .FirstOrDefault();

            await webClient.DownloadFileTaskAsync(asset.BrowserDownloadUrl, savePath);
        }

        public async Task<GitHubRelease> GetLatestReleaseTagAsync(string owner, string repo) {
            var request = new RequestBuilder(API_URL)
                .WithEndpoint($"/repos/{owner}/{repo}/releases/latest")
                .WithMethod(HttpMethod.Get)
                .Build();

            using(var response = await httpClient.SendAsync(request)) {
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GitHubRelease>(content);
            }
        }

        public async Task<GitHubRelease> GetReleaseByTagAsync(string owner, string repo, string tag) {
            var request = new RequestBuilder(API_URL)
                .WithEndpoint($"/repos/{owner}/{repo}/releases/tags/{tag}")
                .WithMethod(HttpMethod.Get)
                .Build();

            using(var response = await httpClient.SendAsync(request)) {
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GitHubRelease>(content);
            }
        }

        public async Task<GitHubRelease[]> GetReleasesAsync(string owner, string repo) {
            var request = new RequestBuilder(API_URL)
                .WithEndpoint($"/repos/{owner}/{repo}/releases")
                .WithMethod(HttpMethod.Get)
                .Build();

            using(var response = await httpClient.SendAsync(request)) {
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GitHubRelease[]>(content);
            }
        }

        public async Task<GithubContent> GetContentFromPathAsync(string owner, string repo, string path) {
            var request = new RequestBuilder(API_URL)
                .WithEndpoint($"/repos/{owner}/{repo}/contents/{path}")
                .WithMethod(HttpMethod.Get)
                .Build();

            using(var response = await httpClient.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                // Check if response is an array (meaning it's a directory)
                if(content.StartsWith("[")) {
                    throw new InvalidOperationException("Response is not a directory");
                }

                return JsonConvert.DeserializeObject<GithubContent>(content);
            }
        }

        public void Dispose() {
            httpClient.Dispose();
            webClient.Dispose();
        }
    }
}
