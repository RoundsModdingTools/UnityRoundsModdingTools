using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ThunderstoreAPI.Entities;
using ThunderstoreAPI.Entities.UserMedia;

namespace ThunderstoreAPI {
    public partial class ThunderstoreApiClient : IDisposable {
        private const string THUNDERSTORE_API_URL = "https://thunderstore.io";

        private readonly Dictionary<string, (Package[], DateTimeOffset)> cachedPackages = new Dictionary<string, (Package[], DateTimeOffset)>();
        private readonly Dictionary<string, Category[]> cachedCategories = new Dictionary<string, Category[]>();

        private readonly RequestBuilder requestBuilder;
        private readonly HttpClient client;
        private readonly TimeSpan cacheDuration;

        public ThunderstoreApiClient(TimeSpan cacheDuration) {
            this.cacheDuration = cacheDuration;
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Add("User-Agent", "UnityRoundsModdingTools");

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

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Package[] packages = JsonConvert.DeserializeObject<Package[]>(content);
            cachedPackages[community] = (packages, DateTimeOffset.Now);

            response.EnsureSuccessStatusCode();
            return packages;
        }

        public async Task DownloadPackageAsync(Package package, string downloadPath, string targetVersion = null) {
            if (targetVersion != null && !package.Versions.Any(v => v.VersionNumber == targetVersion)) {
                throw new ArgumentException("The specified version does not exist in the package.", nameof(targetVersion));
            }

            PackageVersion version = targetVersion != null
                ? package.Versions.FirstOrDefault(v => v.VersionNumber == targetVersion)
                : package.Versions.FirstOrDefault(v => v.IsActive);

            using(var response = await client.GetAsync(version.DownloadUrl)) {
                response.EnsureSuccessStatusCode();
                using(var fileStream = System.IO.File.Create(downloadPath)) {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
        }

        public async Task<Category[]> GetCategoriesAsync(string community) {
            if(cachedCategories.TryGetValue(community, out Category[] cachedCategoriesEntry)) {
                return cachedCategoriesEntry;
            }

            var request = requestBuilder
                .StartNew()
                .WithEndpoint($"/api/experimental/community/{community}/category/")
                .WithMethod(HttpMethod.Get)
                .Build();

            using(var response = await client.SendAsync(request)) {
                var content = await response.Content.ReadAsStringAsync();

                CategoryResponse categoryResponse = JsonConvert.DeserializeObject<CategoryResponse>(content);
                cachedCategories[community] = categoryResponse.Categories;

                response.EnsureSuccessStatusCode();

                return categoryResponse.Categories;
            }
        }

        public async Task PublishAsync(PublishOption publishOption, string path, string token) {
            Byte[] data = System.IO.File.ReadAllBytes(path);
            await PublishAsync(publishOption, data, Path.GetFileName(path), token);
        }

        public async Task PublishAsync(PublishOption publishOption, Byte[] data, string fileName, string token) {
            if(string.IsNullOrWhiteSpace(publishOption.AuthorName)) {
                throw new ArgumentNullException("Author name must not be null or empty.", nameof(publishOption.AuthorName));
            } else if(publishOption.Communities == null || publishOption.Communities.Length == 0) {
                throw new ArgumentNullException("Communities must not be null or empty.", nameof(publishOption.Communities));
            }

            var uploadResponse = await InitiateUploadAsync(fileName, data.LongLength, token);

            Guid uuid = uploadResponse.UserMedia.UUID;

            // Upload the data in parallel
            var uploadTasks = uploadResponse.UploadUrls
                .Select(part => UploadChuckAsync(part, data))
                .ToList();

            CompletedPart[] parts;
            try {
                parts = await Task.WhenAll(uploadTasks);
            } catch(Exception ex) {
                _ = Task.Run(() => AbortUploadAsync(uuid, token));
                throw new Exception("Failed to upload file", ex);
            }

            await FinishUploadAsync(parts, uuid, token);
            await SubmitPackageAsync(uuid, publishOption, token);
        }

        private async Task<UserMediaInitiateUploadResponse> InitiateUploadAsync(string filename, long fileSizeBytes, string token) {
            var request = requestBuilder
                .StartNew()
                .WithEndpoint("/api/experimental/usermedia/initiate-upload/")
                .WithAuth(new AuthenticationHeaderValue("Bearer", token))
                .WithMethod(HttpMethod.Post)
                .WithContent(new StringContent(JsonConvert.SerializeObject(new {
                    filename,
                    file_size_bytes = fileSizeBytes, // Changed to file_size_bytes
                }), Encoding.UTF8, "application/json"))
                .Build();

            using(var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var uploadResponse = JsonConvert.DeserializeObject<UserMediaInitiateUploadResponse>(responseContent);
                return uploadResponse;
            }
        }


        public async Task<CompletedPart> UploadChuckAsync(UploadPartUrl part, Byte[] data) {
            int start = part.Offset;
            int end = start + part.Length;

            Byte[] chunk = new Byte[end - start];
            Array.Copy(data, start, chunk, 0, end - start);

            var request = requestBuilder
                .StartNew()
                .WithAbsoluteEndpoint(part.Url.ToString())
                .WithMethod(HttpMethod.Put)
                .WithContent(new ByteArrayContent(chunk))
                .WithContentType("application/octet-stream")
                .Build();

            using(var response = await client.SendAsync(request)) {
                if(!response.IsSuccessStatusCode) {
                    throw new InvalidOperationException($"Upload failed with status code {response.StatusCode}");
                }

                if(response.Headers.TryGetValues("ETag", out var values)) {
                    var eTag = values.FirstOrDefault();
                    if(eTag != null) {
                        return new CompletedPart {
                            ETag = eTag,
                            PartNumber = part.PartNumber,
                        };
                    }
                }
            }
            throw new InvalidOperationException("ETag not found in the response.");

        }

        private async Task FinishUploadAsync(CompletedPart[] completedParts, Guid uuid, string token) {
            var request = requestBuilder
                .StartNew()
                .WithEndpoint($"/api/experimental/usermedia/{uuid}/finish-upload")
                .WithAuth(new AuthenticationHeaderValue("Bearer", token))
                .WithMethod(HttpMethod.Post)
                .WithContent(new StringContent(JsonConvert.SerializeObject(new {
                    parts = completedParts,
                }), Encoding.UTF8, "application/json"))
                .Build();

            using(var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task AbortUploadAsync(Guid uuid, string token) {
            var request = requestBuilder
                .StartNew()
                .WithEndpoint($"/api​/experimental​/usermedia​/{uuid}​/abort-upload​")
                .WithAuth(new AuthenticationHeaderValue("Bearer", token))
                .WithMethod(HttpMethod.Post)
                .WithContent(new StringContent(JsonConvert.SerializeObject(uuid),
                    Encoding.UTF8,
                    "application/json")
                )
                .Build();

            using(var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task SubmitPackageAsync(Guid uuid, PublishOption publishOption, string token) {
            var metadata = new PackageSubmissionMetadata {
                AuthorName = publishOption.AuthorName,
                Categories = publishOption.Categories,
                Communities = publishOption.Communities,
                HasNSFWContent = publishOption.HasNSFWContent,
                CommunityCategories = publishOption.CommunityCategories,
                UploadUUID = uuid,
            };

            var request = requestBuilder
                .StartNew()
                .WithEndpoint("/api/experimental/submission/submit")
                .WithAuth(new AuthenticationHeaderValue("Bearer", token))
                .WithMethod(HttpMethod.Post)
                .WithContent(new StringContent(JsonConvert.SerializeObject(metadata), Encoding.UTF8, "application/json"))
                .Build();

            using(var response = await client.SendAsync(request)) {
                if(response.StatusCode == HttpStatusCode.BadRequest) {
                    var errorMessage = await HandleBadRequestAsync(response);
                    if(!string.IsNullOrWhiteSpace(errorMessage)) {
                        throw new Exception(errorMessage);
                    }
                }

                if(response.IsSuccessStatusCode) {
                    return;
                }
            }

            throw new InvalidOperationException("Unexpected exception while submitting package.");
        }

        private async Task<string> HandleBadRequestAsync(HttpResponseMessage responseMessage) {
            var responseBody = await responseMessage.Content.ReadAsStringAsync();
            var errorObj = JsonConvert.DeserializeObject<ErrorResponse?>(responseBody);

            if(errorObj?.File == null || !errorObj.Value.File.Any()) {
                return null;
            }

            // For each error, split on ':' to extract the message (if possible)
            var messages = errorObj.Value.File.Select(err => {
                var parts = err.Split(new[] { ':' }, 2);
                return parts.Length == 2 ? parts[1].Trim() : err;
            });

            return string.Join(", ", messages);
        }


        public void Dispose() {
            client.Dispose();
        }
    }
}
