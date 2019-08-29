namespace Zyin.IntentBot.Dialog
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Logging;
    using Zyin.IntentBot.Intent;
    using Zyin.IntentBot.Services;

    /// <summary>
    /// Main dialog driving the main conversation flow with LUIS intent understanding
    /// </summary>
    public class IntentDialog : IntentDialogBase
    {
        /// <summary>
        /// Luis service reference
        /// </summary>
        protected readonly IIntentService intentService;

        /// <summary>
        /// Initializes a new LuisIntentDialog
        /// </summary>
        /// <param name="intentService"></param>
        /// <param name="intentFactory"></param>
        /// <param name="intentHandlerFactory"></param>
        /// <param name="logger"></param>
        public IntentDialog(
            IIntentService intentService,
            IIntentFactory intentFactory,
            IntentHandlerFactory intentHandlerFactory,
            OAuthDialog oAuthDialog,
            ILogger<IntentDialog> logger)
            : base(nameof(IntentDialog), intentFactory, intentHandlerFactory, oAuthDialog, logger)
        {
            this.intentService = intentService;
        }

        /// <summary>
        /// map turn context to an intent string with Luis service
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns></returns>
        protected override async Task<string> GetIntent(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Gather potential intent. (Note the TurnContext has the response to the prompt.)
            return await this.intentService.RunQuery(turnContext, cancellationToken);
        }
    }
}
