using Assets.Plugins.UnityRoundsModdingTools.Editor.Wrappers.ThunderstoreAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ThunderstoreAPI.Entities;
using ThunderstoreAPI.Entities.UserMedia;

namespace ThunderstoreAPI {
    public partial class ThunderstoreApiClient : IDisposable {
        private const string THUNDERSTORE_API_URL = "https://thunderstore.io";

        Dictionary<string, (Package[], DateTimeOffset)> cachedPackages = new Dictionary<string, (Package[], DateTimeOffset)>();
        Dictionary<string, Category[]> cachedCategories = new Dictionary<string, Category[]>();

        private RequestBuilder requestBuilder;
        private HttpClient client;
        private TimeSpan cacheDuration;

        public ThunderstoreApiClient(TimeSpan cacheDuration) {
            this.cacheDuration = cacheDuration;
            this.client = new HttpClient();

            requestBuilder = new RequestBuilder(THUNDERSTORE_API_URL);
        }

        public async Task<Package[]> GetPackagesAsync(string community) {
            if(cachedPackages.TryGetValue(community, out (Package[], DateTimeOffset) cachedPackagesEntry)) {
                if(DateTimeOffset.Now - cachedPackagesEntry.Item2 < cacheDuration) {
                    return cachedPackagesEntry.Item1;
                }
            }

            var request = requestBuilder
                .StartNew()
                .WithEndpoint($"/c/{community}/api/v1/package/")
                .WithMethod(HttpMethod.Get)
                .Build();

            using var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Package[] packages = JsonConvert.DeserializeObject<Package[]>(content);
            cachedPackages[community] = (packages, DateTimeOffset.Now);

            response.EnsureSuccessStatusCode();
            return packages;
        }

        public async Task<Category[]> GetCategoriesAsync() {
            if(cachedCategories.TryGetValue("categories", out Category[] cachedCategoriesEntry)) {
                return cachedCategoriesEntry;
            }

            var request = requestBuilder
                .StartNew()
                .WithEndpoint("/api/v1/category/")
                .WithMethod(HttpMethod.Get)
                .Build();

            using var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Category[] categories = JsonConvert.DeserializeObject<Category[]>(content);
            cachedCategories["categories"] = categories;

            response.EnsureSuccessStatusCode();
            return categories;
        }

        public async Task<UserMediaInitiateUploadResponse> InitiateUploadAsync(string filename, int fileSizeBytes, string token) {
            var request = requestBuilder
                .StartNew()
                .WithEndpoint("/api/experimental/usermedia/initiate-upload/")
                .WithAuth(new AuthenticationHeaderValue("Bearer", token))
                .WithMethod(HttpMethod.Post)
                .WithContent(new StringContent(JsonConvert.SerializeObject(new {
                    filename,
                    fileSizeBytes,
                }), Encoding.UTF8, "application/json"))
                .Build();

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var uploadResponse = JsonConvert.DeserializeObject<UserMediaInitiateUploadResponse>(responseContent);
            return uploadResponse;
        }


        public async Task<CompletedPart> UploadChuckAsync(UploadPartUrl part, Byte[] data) {
            int start = part.Offset;
            int end = start + part.Length;

            Byte[] chunk = new Byte[end - start];
            Array.Copy(data, start, chunk, 0, end - start);

            var request = requestBuilder
                .StartNew()
                .WithEndpoint(part.Url.ToString())
                .WithMethod(HttpMethod.Put)
                .WithContent(new ByteArrayContent(chunk))
                .WithContentType("application/octet-stream")
                .Build();

            using var response = await client.SendAsync(request);

            if(response.Headers.TryGetValues("ETag", out var values)) {
                var eTag = values.FirstOrDefault();
                if(eTag != null) {
                    return new CompletedPart {
                        ETag = eTag,
                        PartNumber = part.PartNumber,
                    };
                }
            }

            throw new InvalidOperationException("ETag not found in the response.");
        }

        public async Task FinishUploadAsync(List<CompletedPart> completedParts, Guid guid, string token) {
            var request = requestBuilder
                .StartNew()
                .WithEndpoint($"{THUNDERSTORE_API_URL}/api​/experimental​/usermedia​/{guid}​/finish-upload​")
                .WithAuth(new AuthenticationHeaderValue("Bearer", token))
                .WithMethod(HttpMethod.Post)
                .WithContent(new StringContent(JsonConvert.SerializeObject(new {
                    parts = completedParts,
                }), Encoding.UTF8, "application/json"))
                .Build();

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task AbortUploadAsync(Guid uuid, string token) {
            var request = requestBuilder
                .StartNew()
                .WithEndpoint($"{THUNDERSTORE_API_URL}/api​/experimental​/usermedia​/{uuid}​/abort-upload​")
                .WithAuth(new AuthenticationHeaderValue("Bearer", token))
                .WithMethod(HttpMethod.Post)
                .WithContent(new StringContent(JsonConvert.SerializeObject(uuid),
                    Encoding.UTF8,
                    "application/json")
                )
                .Build();

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose() {
            client.Dispose();
        }
    }
}
