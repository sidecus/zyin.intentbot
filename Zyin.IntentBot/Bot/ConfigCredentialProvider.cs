namespace Zyin.IntentBot.Bot
{
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Options;
    using Zyin.IntentBot.Config;

    /// <summary>
    /// Bot credential provider based on config
    /// </summary>
    public class ConfigCredentialProvider : SimpleCredentialProvider
    {
        /// <summary>
        /// Initializes a new instance of ConfigCredentialProvider
        /// </summary>
        /// <param name="options"></param>
        public ConfigCredentialProvider(IOptions<BotConfig> options)
            : base(options.Value.MicrosoftAppId, options.Value.MicrosoftAppPassword)
        {
        }
    }
}
