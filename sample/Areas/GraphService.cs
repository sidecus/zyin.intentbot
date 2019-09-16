namespace sample.Areas
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Zyin.IntentBot.Services;

    /// <summary>
    /// Graph service which talks to graph apis
    /// </summary>
    public class GraphService : AADv2HttpService
    {
        public static readonly string BaseUri = "https://graph.microsoft.com";
        protected static readonly string MyProfileUri = "/v1.0/me/";
        protected static readonly string UserProfileUri = "/v1.0/users/{0}";
        protected static readonly string ProfilePhotoUri = "/v1.0/me/photos/64x64/$value";
        protected static readonly string Default64x64ProfilePhoto = "data:image/png;base64,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";

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphService" /> class
        /// </summary>
        /// <param name="httpClient">the required graph api http client</param>
        /// <param name="logger">logger for this class</param>
        public GraphService(HttpClient httpClient, ILogger<GraphService> logger)
            : base(httpClient, logger)
        {
        }

        /// <summary>
        /// Get my profile
        /// </summary>
        /// <throws>HttpRequestException</throws>
        public async Task<(string name, string mail, string mobilePhone, string title, Guid id)> GetMyProfile()
        {
            return await this.GetUserProfileInternalAsync(MyProfileUri);
        }

        /// <summary>
        /// Get another user's profile
        /// </summary>
        /// <throws>HttpRequestException</throws>
        public async Task<(string name, string mail, string mobilePhone, string title, Guid id)> GetUserProfileAsync(string upn)
        {
            if (string.IsNullOrWhiteSpace(upn))
            {
                throw new ArgumentNullException(nameof(upn));
            }

            return await this.GetUserProfileInternalAsync(string.Format(UserProfileUri, upn));
        }

        /// <summary>
        /// Get my photo in base 64 data url format
        /// </summary>
        /// <returns>inline base64 image</returns>
        public async Task<string> GetMyPhoto()
        {
            return await this.GetHttpAsync(
                ProfilePhotoUri,
                async response =>
                {
                    var img = await response.Content.ReadAsByteArrayAsync();

                    // Convert to base 64 encoded data uri format
                    var type = response.Content.Headers.ContentType.MediaType;
                    var base64Img = System.Convert.ToBase64String(img);
                    return $"data:{type};base64,{base64Img}";
                },
                response => Task.FromResult(Default64x64ProfilePhoto)
            );
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        /// <throws>HttpRequestException</throws>
        private async Task<(string name, string mail, string mobilePhone, string title, Guid id)> GetUserProfileInternalAsync(string uri)
        {
            return await this.GetHttpAsync(
                uri,
                async response =>
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jObj = JObject.Parse(json);
                    return
                    (
                        (string) jObj.SelectToken("displayName"),
                        (string) jObj.SelectToken("mail"),
                        (string) jObj.SelectToken("mobilePhone"),
                        (string) jObj.SelectToken("jobTitle"),
                        (Guid) jObj.SelectToken("id")
                    );
                });
        }
    }
}
