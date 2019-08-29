namespace Zyin.IntentBot.Config
{
    /// <summary>
    /// OAuth config if the bot uses authentication
    /// </summary>
    public class OAuthConfig
    {
        /// <summary>
        /// Gets the OAuth connection name you configured in Azure Bot Service
        /// </summary>
        public string OAuthConnectionName { get; set; }

        /// <summary>
        /// Gets the AAD app Id (if you use authentication and token exchange)
        /// </summary>
        public string AADAppId { get; set; }

        /// <summary>
        /// Gets the AAD app Id (if you use authentication and token exchange)
        /// </summary>
        public string AADAppPassword { get; set; }
    }
}