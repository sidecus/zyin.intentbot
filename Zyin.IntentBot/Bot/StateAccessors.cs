namespace Zyin.IntentBot.Bot
{
    using System;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    /// <summary>
    /// Singleton class to get required user state accessors
    /// </summary>
    public class UserStateAccessors<TUserInfo>
        where TUserInfo : UserInfo
    {
        /// <summary>
        /// User state reference
        /// </summary>
        private readonly UserState userState;
        
        /// <summary>
        /// Initializes a new instance of UserStateAccessors
        /// </summary>
        /// <param name="userState"></param>
        public UserStateAccessors(UserState userState)
        {
            this.userState = userState ?? throw new ArgumentNullException(nameof(userState));
            this.UserInfoAccessor = this.userState.CreateProperty<TUserInfo>(typeof(TUserInfo).Name);
        }

        /// <summary>
        /// User state accessor.
        /// </summary>
        /// <value></value>
        public IStatePropertyAccessor<TUserInfo> UserInfoAccessor { get; private set; }
    }

    /// <summary>
    /// Singleton class to get required dialog state accessors
    /// </summary>
    public class DialogStateAccessors
    {
        /// <summary>
        /// Conversation state reference
        /// </summary>
        private readonly ConversationState conversationState;

        /// <summary>
        /// Initializes a new instance of StateAccessors
        /// </summary>
        /// <param name="conversationState"></param>
        /// <param name="userState"></param>
        public DialogStateAccessors(ConversationState conversationState)
        {
            this.conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            this.DialogStateAccessor = this.conversationState.CreateProperty<DialogState>(nameof(DialogState));
        }

        /// <summary>
        /// Dialog state accessor based off the ConversationState. Used by the Dialog subsystem.
        /// </summary>
        /// <value></value>
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; private set; }
    }
}