namespace Zyin.IntentBot.Bot
{
    /// <summary>
    /// Basic user info
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// name claim value
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// UPN claim value
        /// </summary>
        public string UserPrincipalName { get; set; }
    }
}