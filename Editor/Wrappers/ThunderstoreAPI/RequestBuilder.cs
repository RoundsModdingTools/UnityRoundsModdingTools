using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Assets.Plugins.UnityRoundsModdingTools.Editor.Wrappers.ThunderstoreAPI {
    public class RequestBuilder {
        private UriBuilder uriBuilder = new UriBuilder() {
            Scheme = "https"
        };
        public HttpMethod Method = HttpMethod.Get;
        public AuthenticationHeaderValue Authorization;
        public HttpContent Content;

        public RequestBuilder(string host) {
            uriBuilder.Host = host.StartsWith("https://")
                ? host.Substring(8)
                : host;
        }

        public RequestBuilder StartNew() {
            return new(uriBuilder.Uri.Host);
        }

        public RequestBuilder WithEndpoint(string endpoint) {
            uriBuilder.Path = endpoint.EndsWith("/")
                ? endpoint
                : endpoint + "/";

            return this;
        }

        public RequestBuilder WithAuth(AuthenticationHeaderValue authorization) {
            Authorization = authorization;
            return this;
        }

        public RequestBuilder WithContent(HttpContent content) {
            Content = content;
            return this;
        }

        public RequestBuilder WithMethod(HttpMethod method) {
            Method = method;
            return this;
        }

        public RequestBuilder WithContentType(string contentType) {
            Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return this;
        }

        public HttpRequestMessage Build() {
            var request = new HttpRequestMessage(Method, uriBuilder.Uri) {
                Content = Content
            };

            request.Headers.Authorization = Authorization;

            return request;
        }
    }
}
