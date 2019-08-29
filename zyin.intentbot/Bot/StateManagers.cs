namespace Zyin.IntentBot.Bot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    /// <summary>
    /// Singleton class to get required state accessors
    /// </summary>
    public class StateAccessorManager<TBotState, TStateType>
        where TBotState : BotState
        where TStateType: class, new()
    {
        /// <summary>
        /// Bot state reference (ConversationState or UserState)
        /// </summary>
        private readonly TBotState botState;

        /// <summary>
        /// Initializes a new instance of StateAccessors
        /// </summary>
        /// <param name="botState"></param>
        public StateAccessorManager(TBotState botState)
        {
            this.botState = botState ?? throw new ArgumentNullException(nameof(botState));
            this.StateAccessor = this.botState.CreateProperty<TStateType>(typeof(TStateType).Name);
        }

        /// <summary>
        /// State property accessor based off the given bot state
        /// </summary>
        public IStatePropertyAccessor<TStateType> StateAccessor { get; private set; }

        /// <summary>
        /// Convenient method to get the state object with default factory
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TStateType> GetAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return this.StateAccessor.GetAsync(turnContext, () => new TStateType(), cancellationToken);
        }

        /// <summary>
        /// Convenient method to set object into the state
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetAsync(ITurnContext turnContext, TStateType value, CancellationToken cancellationToken)
        {
            return this.StateAccessor.SetAsync(turnContext, value, cancellationToken);
        }
    }

    /// <summary>
    /// User state manager
    /// </summary>
    /// <typeparam name="TStateType"></typeparam>
    public class UserStateManager<TStateType> : StateAccessorManager<UserState, TStateType>
        where TStateType : class, new()
    {
        public UserStateManager(UserState userState) : base(userState) {}
    }

    /// <summary>
    /// User token state manager
    /// </summary>
    public class UserTokenStateManager : UserStateManager<UserTokenInfo>
    {
        public UserTokenStateManager(UserState userState) : base(userState) {}
    }

    /// <summary>
    /// Custom conversataion state manager
    /// </summary>
    public class ConversationStateManager<TStateType> : StateAccessorManager<ConversationState, TStateType>
        where TStateType : class, new()
    {
        public ConversationStateManager(ConversationState conversationState) : base(conversationState) {}
    }

    /// <summary>
    /// Dialog state is from conversataion state
    /// </summary>
    public class DialogStateManager : ConversationStateManager<DialogState>
    {
        public DialogStateManager(ConversationState conversationState) : base(conversationState) {}
    }
}