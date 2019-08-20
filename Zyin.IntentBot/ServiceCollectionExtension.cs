namespace Zyin.IntentBot
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Zyin.IntentBot.Bot;
    using Zyin.IntentBot.Dialog;
    using Zyin.IntentBot.Intent;
    using Zyin.IntentBot.Services;
    using Zyin.IntentBot.Prompt;

    /// <summary>
    /// service collection extension to register intent factory and intent handler factory
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Add intent factory, intent handler factory, as well as fall back intent and handler
        /// </summary>
        /// <param name="services">service collection</param>
        /// <typeparam name="TBot">type of bot</param>
        /// <returns>service collection</returns>
        public static IServiceCollection AddIntentBot<TBot>(this IServiceCollection services)
            where TBot : DialogBot<IntentDialog>
        {
            return services.AddIntentBot<TBot, UserInfo>();
        }

        /// <summary>
        /// Add intent factory, intent handler factory, as well as fall back intent and handler.
        /// User can specify a TUserInfo for user state
        /// </summary>
        /// <param name="services">service collection</param>
        /// <typeparam name="TBot">type of bot</param>
        /// <typeparam name="TUser">type of user info to save into UserState</param>
        /// <returns>service collection</returns>
        public static IServiceCollection AddIntentBot<TBot, TUser>(this IServiceCollection services)
            where TBot : DialogBot<IntentDialog>
            where TUser : UserInfo
        {
            // Add required/optional services for the bot
            // 1. File content service for json cards
            // 2. aadv2 token exchange service
            services.AddSingleton<JsonAdaptiveCardService>();
            services.AddSingleton<IAADv2OBOTokenService, AADv2OBOTokenService>();

            // Add intent bot
            services.AddIntentBotInternal<TBot, TUser>();

            // Add singleton intent factory and intent handler factory
            services.AddSingleton<IIntentFactory, IntentFactory>();
            services.AddSingleton<IntentHandlerFactory>();
            
            // Add default fallback intent and handler.
            services.AddSimpleIntent<FallbackContext, DefaultFallbackIntentHandler>();

            return services;
        }

        /// <summary>
        /// Add a simple intent which doesn't require user input
        /// </summary>
        /// <param name="services">services</param>
        /// <typeparam name="TContext">Simple intent context</typeparam>
        /// <typeparam name="THandler">intent handler</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddSimpleIntent<TContext, THandler>(this IServiceCollection services)
            where TContext: IntentContext, new()
            where THandler: IntentHandler<TContext>
        {
            // Add singleton intent and transient intent handler.
            // Intent context is not registered, but will be created on the fly (hence transient just not via DI) for each new conversation.
            services.AddSingleton<IIntent, Intent<TContext>>();
            services.AddTransient<IntentHandler<TContext>, THandler>();

            return services;
        }

        /// <summary>
        /// Add an intent which requires a dialog to collect more information
        /// </summary>
        /// <param name="services">services</param>
        /// <typeparam name="TContext">Simple intent context</typeparam>
        /// <typeparam name="THandler">intent handler</typeparam>
        /// <returns></returns>
        public static IServiceCollection AddUserInputIntent<TContext, THandler>(this IServiceCollection services)
            where TContext: IntentContext, new()
            where THandler: IntentHandler<TContext>
        {
            // Add singleton user input intent and transient intent handlers.
            // User input intent require a few other things, including singleton prompt manager and singleton user input dialog.
            // Register them as well.
            services.AddSingleton<PromptManager<TContext>>();
            services.AddSingleton<UserInputDialog<TContext>>();
            services.AddSingleton<IIntent, UserInputIntent<TContext>>();
            services.AddTransient<IntentHandler<TContext>, THandler>();

            return services;
        }

        /// <summary>
        /// Add Dialogs, bot state, bot adapter, and bot
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TBot">type of bot</param>
        /// <typeparam name="TUser">type of user info to save into UserState</param>
        /// <returns></returns>
        private static IServiceCollection AddIntentBotInternal<TBot, TUser>(this IServiceCollection services)
            where TBot : DialogBot<IntentDialog>
            where TUser : UserInfo
        {
            // Register main dialogs.
            services.AddSingleton<OAuthDialog>();
            services.AddSingleton<IntentDialog>();

            // Create the Conversation state and the User state.
            // Also register singleton dialog state accessors and user state accessors with default user info.
            services.AddSingleton<ConversationState>();
            services.AddSingleton<UserState>();
            services.AddSingleton<DialogStateAccessors>();
            services.AddSingleton<UserStateAccessors<TUser>>();

            // By default use memory storage. Real implementation should register a different IStorage.
            services.AddSingleton<IStorage, MemoryStorage>();

            // Register the credential provider to be used with the Bot Framework Adapter.
            // Register the Adapter to process HTTP requests.
            // Add bot (the bot is transient, however the main dialog is actually singleton)
            services.AddSingleton<ICredentialProvider, ConfigCredentialProvider>();
            services.AddSingleton<IBotFrameworkHttpAdapter, IntentBotAdapter>();
            services.AddTransient<IBot, TBot>();

            return services;
        }
    }
}