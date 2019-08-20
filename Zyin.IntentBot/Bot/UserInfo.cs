namespace Zyin.IntentBot.Bot
{
    /// <summary>
    /// Base class for user related information to be saved into state.
    /// If you have more user state to save:
    /// 1. Derive your own class from UserInfo
    /// 2. Register it in DI container via services.AddSingleton<StateAccessors<UserInfo>>();
    /// 3. Use it in your own class via DI by accessing StateAccessors.UserInfoAccessor.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// User name
        /// </summary>
        public string Name { get; set; }
    }
}