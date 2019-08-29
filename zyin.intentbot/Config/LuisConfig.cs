namespace Zyin.IntentBot.Config
{
    /// <summary>
    /// Luis config
    /// </summary>
    public class LuisConfig
    {
        /// <summary>
        /// Gets the luis app id
        /// </summary>
        public string LuisAppId { get; set; }

        /// <summary>
        /// Gets the luis app key
        /// </summary>
        public string LuisApiKey { get; set; }

        /// <summary>
        /// Get the Luis API Host Name
        /// </summary>
        /// <value></value>
        public string LuisApiEndPoint { get; set; }
    }
}