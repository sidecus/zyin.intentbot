namespace Zyin.IntentBot.Dialog
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Zyin.IntentBot.Config;
    using Zyin.IntentBot.Intent;
    
    /// <summary>
    /// OAuth dialog to get OAuth token
    /// </summary>
    public class OAuthDialog : ComponentDialog
    {
        /// <summary>
        /// ILogger instance
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// OAuth prompt settings
        /// </summary>
        protected readonly OAuthPromptSettings oAuthPromptSettings;

        /// <summary>
        /// Initializes a new instance of the OAuthDialog class.
        /// This constructor will be used in DI.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public OAuthDialog(IOptions<OAuthConfig> options, ILogger<OAuthDialog> logger)
            : this(nameof(OAuthDialog), options.Value.OAuthConnectionName, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the OAuthDialog class
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="oAuthConnectionName"></param>
        /// <param name="logger">logger</param>
        public OAuthDialog(string dialogId, string oAuthConnectionName, ILogger logger)
            : base(dialogId)
        {
            this.logger = logger;
            this.oAuthPromptSettings = new OAuthPromptSettings()
            {
                ConnectionName = oAuthConnectionName,
                Text = "Authentication required - we need you to sign in to proceed with this task.",
                Title = "Click to sign in",
                Timeout = 2 * 60 * 1000,    // 2 minutes
            };

            // Add common prompts
            AddDialog(new OAuthPrompt(nameof(OAuthPrompt), this.oAuthPromptSettings));

            // Add main flow waterfall
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                StartAuthStepAsync,
                EnsureValidTokenStepAsync,
                GetTokenStepAsync
            }));

            // Set the initial child Dialog to the waterfall dialog
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Helper method to sign user out
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="connectionName"></param>
        /// <param name="logger"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task SignOutAsync(ITurnContext turnContext, string connectionName, ILogger logger, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (!(turnContext.Adapter is IUserTokenProvider adapter))
            {
                throw new InvalidOperationException("sign out not supported by current adapter");
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentNullException(nameof(connectionName));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var userId = turnContext.Activity?.From?.Id;

            // Sign user out
            logger.LogInformation($"Signing out user {userId}.");
            await adapter.SignOutUserAsync(turnContext, connectionName, userId, cancellationToken);
        }

        /// <summary>
        /// Authenticate the user
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> StartAuthStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Start auth flow
            this.logger.LogInformation("Starting OAuth Prompt.");
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        /// <summary>
        /// Azure bot service might return expired token. Add this step to ensure it's valid by logging user out and ask for reauth
        /// </summary>
        /// <param name="stepContext">step context</param>
        /// <param name="cancellationToken"></param>
        /// <returns>dialog turn result</returns>
        private async Task<DialogTurnResult> EnsureValidTokenStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = stepContext.Result as TokenResponse;
            if (tokenResponse != null)
            {
                var jwtToken = new JwtSecurityToken(tokenResponse.Token);
                if (jwtToken.ValidTo <= DateTime.UtcNow.AddMinutes(5))
                {
                    // token has expired. This is a bug in Azure bot service.
                    // We need to work around it by logging user out first and then ask for logging in again
                    var userId = stepContext.Context.Activity?.From?.Id;
                    this.logger.LogWarning($"Token for user {userId} expired. Will logout and let user retry. ValidTo:{jwtToken.ValidTo.ToString()}");

                    // Sign user out
                    await OAuthDialog.SignOutAsync(stepContext.Context, this.oAuthPromptSettings.ConnectionName, this.logger, cancellationToken);

                    // Ask user to sign in again by restarting StartAuthStepAsync
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your token expired. Please sign in again."), cancellationToken);
                    return await this.StartAuthStepAsync(stepContext, cancellationToken);
                }
            }

            // For all other cases (success or other authentication failure), move to next step.
            return await stepContext.NextAsync(tokenResponse, cancellationToken);
        }

        /// <summary>
        /// Check authentication result
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> GetTokenStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step (succes or failure), and set it on context
            var intentContext = stepContext.Options as IntentContext;
            var tokenResponse = stepContext.Result as TokenResponse;
            intentContext.TokenResponse = tokenResponse;

            var userId = stepContext.Context.Activity?.From?.Id;
            var channelId = stepContext.Context.Activity?.ChannelId;
            if (tokenResponse != null)
            {
                this.logger.LogInformation($"Get token for user {userId} from channel {channelId}.");
            }
            else
            {
                this.logger.LogError($"Log in failed. No token acquired for {userId} from channel {channelId}.");

                var userMessage = "Login failed or timed out. Please try again.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(userMessage), cancellationToken);
            }

            // Return intent context (which has the token if log in succeeded, or null if it failed).
            return await stepContext.EndDialogAsync(intentContext, cancellationToken: cancellationToken);
        }
    }
}