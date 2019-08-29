namespace Zyin.IntentBot.Bot
{
    using System;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.Teams.Middlewares;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Botframework HTTP adapter with typing/teams and error handling.
    /// </summary>
    public class IntentBotAdapter : BotFrameworkHttpAdapter
    {
        /// <summary>
        /// Initializes a new instance of the IntentBotAdapter class
        /// </summary>
        /// <param name="credentialProvider"></param>
        /// <param name="conversationState"></param>
        /// <param name="userState"></param>
        /// <param name="logger"></param>
        public IntentBotAdapter(ICredentialProvider credentialProvider, ConversationState conversationState, UserState userState, ILogger<BotFrameworkHttpAdapter> logger)
            : base(credentialProvider)
        {
            if (conversationState == null)
            {
                throw new ArgumentNullException(nameof(credentialProvider));
            }

            if (conversationState == null)
            {
                throw new ArgumentNullException(nameof(conversationState));
            }

            if (userState == null)
            {
                throw new ArgumentNullException(nameof(userState));
            }

            // Enalbe state auto save
            this.Use(new AutoSaveStateMiddleware(conversationState, userState));

            // Teams middleware
            this.Use(new TeamsMiddleware(credentialProvider));

            // Configure typing middleware here: https://github.com/microsoft/botbuilder-dotnet/issues/2407            
            this.Use(new ShowTypingMiddleware(200));

            // Add global error handling
            this.OnTurnError = async (turnContext, exception) =>
            {
                // Log any uncaught exception, and send a generic error back to the user.
                logger.LogError(exception, $"Uncaught exception: {exception.Message}");
                await turnContext.SendActivityAsync($"Uh oh, something is wrong: {exception.Message}");

                try
                {
                    // Try to delete clean up the conversation state for the current conversation
                    // to prevent the bot from getting stuck in a error-loop caused by the bad state.
                    await conversationState.DeleteAsync(turnContext);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Exception attempting to Delete ConversationState: {e.Message}");
                }
            };
        }
    }
}
