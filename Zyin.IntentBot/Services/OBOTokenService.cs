namespace Zyin.IntentBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Client;
    using Zyin.IntentBot.Config;

    /// <summary>
    /// Interface for token exchange service to get token for different audiences
    /// </summary>
    public interface IAADv2OBOTokenService
    {
        /// <summary>
        /// Get an OBO token for a new resource
        /// </summary>
        /// <param name="appToken">token for current app</param>
        /// <param name="scopes">new resource scopes</param>
        /// <returns>token for the given resource</returns>
        Task<string> GetOnBehalfOfTokenAsync(string appToken, IEnumerable<string> scopes);
    }

    /// <summary>
    /// Token exchange service to get token for different audiences
    /// </summary>
    public sealed class AADv2OBOTokenService : IAADv2OBOTokenService
    {
        private readonly string aadAppId;
        private readonly string appdAppPassword;
        private readonly IConfidentialClientApplication application;

        /// <summary>
        /// Initializes a new OBOTokenService based on the config
        /// </summary>
        /// <param name="configuration"></param>
        public AADv2OBOTokenService(IOptions<OAuthConfig> options)
        {
            this.aadAppId = options.Value.AADAppId;
            this.appdAppPassword = options.Value.AADAppPassword;

            this.application = ConfidentialClientApplicationBuilder.Create(this.aadAppId)
                .WithClientSecret(this.appdAppPassword)
                .Build();
        }

        /// <summary>
        /// Get an OBO token for a new resource
        /// </summary>
        /// <param name="appToken">token for current app</param>
        /// <param name="scopes">list of scopes</param>
        /// <returns>on behalf of token for the given resource</returns>
        public async Task<string> GetOnBehalfOfTokenAsync(string appToken, IEnumerable<string> scopes)
        {
            if (string.IsNullOrWhiteSpace(appToken))
            {
                throw new ArgumentNullException(nameof(appToken));
            }

            if (scopes == null || !scopes.Any())
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            // Get user assertion with default type "urn:ietf:params:oauth:grant-type:jwt-bearer"
            var userAssertion = new UserAssertion(appToken);
            var result = await this.application.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();

            return result.AccessToken;
        }
    }
}