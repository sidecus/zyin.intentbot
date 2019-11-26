namespace Zyin.IntentBot.Dialog
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Zyin.IntentBot.Bot;
    using Zyin.IntentBot.Config;
    using Zyin.IntentBot.Intent;

    /// <summary>
    /// OAuth dialog to get OAuth token
    /// </summary>
    public class OAuthDialog : ComponentDialog
    {
        /// <summary>
        /// OAuth prompt settings
        /// </summary>
        protected readonly OAuthPromptSettings oAuthPromptSettings;

        /// <summary>
        /// user token manager
        /// </summary>
        protected readonly UserInfoStateManager userInfoStateManager;

        /// <summary>
        /// ILogger instance
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the OAuthDialog class.
        /// This constructor will be used in DI.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="userInfoStateManager"></param>
        /// <param name="logger"></param>
        public OAuthDialog(IOptions<OAuthConfig> options, UserInfoStateManager userInfoStateManager, ILogger<OAuthDialog> logger)
            : base(nameof(OAuthDialog))
        {
            this.userInfoStateManager = userInfoStateManager ?? throw new ArgumentNullException(nameof(userInfoStateManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.oAuthPromptSettings = new OAuthPromptSettings()
            {
                ConnectionName = options?.Value?.OAuthConnectionName ?? throw new ArgumentNullException("OAuthConnectionName"),
                Text = "Authentication required - we need you to sign in to proceed with this task.",
                Title = "Click to sign in",
                Timeout = 30 * 1000,    // 30 seconds
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
        /// Static helper method to sign user out
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
            TokenResponse tokenToReturn = null;

            if (tokenResponse?.Token != null)
            {
                var jwtToken = new JwtSecurityToken(tokenResponse.Token);

                // Check token expiration. 5 minutes is industry stand for clock skew
                if (jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(5))
                {
                    // we have a token but it has expired. This seems to be a bug in Azure bot service.
                    // We need to work around it by logging user out first and then ask for logging in again
                    var userId = stepContext.Context.Activity?.From?.Id;
                    this.logger.LogWarning($"Token for user {userId} expired. Will logout and let user retry.");

                    // Sign user out
                    await OAuthDialog.SignOutAsync(stepContext.Context, this.oAuthPromptSettings.ConnectionName, this.logger, cancellationToken);

                    // Ask user to sign in again by restarting StartAuthStepAsync
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your token expired. Please sign in again."), cancellationToken);
                    return await this.StartAuthStepAsync(stepContext, cancellationToken);
                }

                // Token is good, set it to the return value
                tokenToReturn = tokenResponse;
            }

            // For all other cases (success or other authentication failure), move to next step.
            return await stepContext.NextAsync(tokenToReturn, cancellationToken);
        }

        /// <summary>
        /// Check authentication result
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> GetTokenStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the token from the previous step (succes or failure)
            var intentContext = stepContext.Options as IntentContext;
            var tokenResponse = stepContext.Result as TokenResponse;

            // Try to save the new token (or clear the value if we didn't get a new one)
            await this.SaveUserInfoAsync(stepContext.Context, tokenResponse?.Token, cancellationToken);

            // Check result
            var userId = stepContext.Context.Activity?.From?.Id;
            var channelId = stepContext.Context.Activity?.ChannelId;
            var hasValidToken = tokenResponse?.Token != null;
            if (hasValidToken)
            {
                intentContext.AppToken = tokenResponse.Token;
                this.logger.LogInformation($"Acquired token for user {userId} from channel {channelId} for {intentContext.IntentName}.");
            }
            else
            {
                this.logger.LogError($"Log in failed. No token acquired for {userId} from channel {channelId} for {intentContext.IntentName}.");
                var userMessage = "Login failed or timed out. Please try again.";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(userMessage), cancellationToken);
            }

            // End dialog. If we succeed, we'll return the intent context. Otherwise we'll return null
            return await stepContext.EndDialogAsync(hasValidToken ? intentContext : null, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Save token to user state when needed (whether it's a valid new token or it's empty)
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="token">user token. this can be null</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SaveUserInfoAsync(ITurnContext turnContext, string token, CancellationToken cancellationToken)
        {
            var jwtToken = string.IsNullOrEmpty(token) ? null : new JwtSecurityToken(token);
            var name = jwtToken?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var upn = jwtToken?.Claims.FirstOrDefault(c => c.Type == "upn")?.Value;
            var userInfo = await this.userInfoStateManager.GetAsync(turnContext, cancellationToken);
            if (userInfo.UserName != name || userInfo.UserPrincipalName != upn)
            {
                userInfo.UserName = name;
                userInfo.UserPrincipalName = upn;
                this.logger.LogInformation($"Caching info for user {turnContext.Activity?.From?.Id}, upn: {upn}.");
                await this.userInfoStateManager.SetAsync(turnContext, userInfo, cancellationToken);
            }
        }
    }
}