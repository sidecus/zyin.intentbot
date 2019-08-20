namespace Zyin.IntentBot.Dialog
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Logging;
    using Zyin.IntentBot.Intent;

    /// <summary>
    /// Main bot dialog class leverages Intent/IntentFactory to understand user intent and drive dialog flow
    /// </summary>
    public abstract class IntentDialogBase : ComponentDialog
    {
        protected readonly IIntentFactory intentFactory;
        protected readonly IntentHandlerFactory intentHandlerFactory;
        protected readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the IntentDialog class
        /// </summary>
        /// <param name="dialogId"></param>
        /// <param name="intentFactory"></param>
        /// <param name="logger"></param>
        public IntentDialogBase(
            string dialogId,
            IIntentFactory intentFactory,
            IntentHandlerFactory intentHandlerFactory,
            OAuthDialog oAuthDialog,
            ILogger logger)
            : base(dialogId)
        {
            this.intentFactory = intentFactory;
            this.intentHandlerFactory = intentHandlerFactory;
            this.logger = logger;

            // Add common prompts
            this.AddDialog(new TextPrompt(nameof(TextPrompt)));
            this.AddDialog(oAuthDialog);

            // Add main flow waterfall
            var warterfallSteps = new WaterfallStep[]
            {
                GetIntentStepAsync,
                GetUserInputStepAsync,
                ExecuteTaskStepAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), warterfallSteps));

            // Add children dialogs required by the intent factory
            foreach (var dialog in this.intentFactory.Dialogs)
            {
                AddDialog(dialog);
            }

            // Set the initial child Dialog to the waterfall dialog
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// abstract method to map turn context to an intent string
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>intent string</returns>
        protected abstract Task<string> GetIntent (ITurnContext turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// First step in the waterfall to get intent and then collect additional info as appropriate
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> GetIntentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = stepContext.Context.Activity.AsMessageActivity();
            var query = message?.Text;

            // Get intent from user input
            var intentString = await this.GetIntent(stepContext.Context, cancellationToken);

            // Get intent and intent context instances
            var intentContext = this.intentFactory.CreateIntentContext(intentString, query);

            // Check whether we need auth
            if (intentContext.RequireAuth)
            {
                // Start auth flow
                return await stepContext.BeginDialogAsync(nameof(OAuthDialog), intentContext, cancellationToken);
            }
            else
            {
                // No auth required, go to next step
                return await stepContext.NextAsync(intentContext, cancellationToken);
            }
        }

        /// <summary>
        /// Get user input
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> GetUserInputStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var intentContext = stepContext.Result as IntentContext;
            if (intentContext.RequireAuth && intentContext?.AppToken == null)
            {
                // Auth didn't succeed. End dialog
                this.logger.LogWarning($"{intentContext.GetType()} requires auth but auth didn't succeed");
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            if (intentContext.HasDialog)
            {
                // Known intent with dialog, run the subsequent dialog based to collect additional info
                return await stepContext.BeginDialogAsync(intentContext.DialogId, intentContext, cancellationToken);
            }
            else
            {
                // intent which can be processed right away (simiple ones or fall back), move on to next step
                return await stepContext.NextAsync(intentContext, cancellationToken);
            }
        }

        /// <summary>
        /// Given the collected intent context, try to run the corresponding task
        /// </summary>
        /// <param name="stepContext">step context</param>
        /// <param name="cancellationToken"></param>
        /// <returns>dialog turn result</returns>
        private async Task<DialogTurnResult> ExecuteTaskStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // 1. If it's a known intent or fallback intent, stepContext.Result is a regular IntentContext
            // 2. If the child dialog was cancelled or the user failed to confirm, stepContext.Result will be null.
            // Below branches will handle them accordingly
            var intentContext = stepContext.Result as IntentContext;

            if (intentContext != null)
            {
                // Process request
                await this.intentHandlerFactory.ProcessIntentAsync(stepContext.Context, intentContext, cancellationToken);
            }
            else
            {            
                // User canceled or abandoned the conversation
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            }

            // We are done. End the current dialog
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}