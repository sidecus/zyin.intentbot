namespace Zyin.IntentBot.Bot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;

    /// <summary>
    /// Bot which uses a main dialog to orchestrate the main flow
    /// </summary>
    /// <typeparam name="T">Dialog type</typeparam>
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        private static readonly string TeamsChannelId = "msteams";
        private static readonly string TeamsSignInActivityName = "signin/verifyState";
        protected readonly Dialog dialog;
        protected readonly DialogStateAccessors dialogStateAccessors;
        protected readonly ILogger logger;

        /// <summary>
        /// Initializes a new dialog bot
        /// </summary>
        /// <param name="conversationState"></param>
        /// <param name="userState"></param>
        /// <param name="dialog"></param>
        /// <param name="logger"></param>
        public DialogBot(DialogStateAccessors stateAccessors, T dialog, ILogger logger)
        {
            this.dialogStateAccessors = stateAccessors ?? throw new ArgumentNullException(nameof(stateAccessors));
            this.dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));;
            this.logger = logger;
        }

        /// <summary>
        /// override to handle each turn activity
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Call base onturn first
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Forward sign in Invoke activity (for Teams OAuth) to underlying dialog.
            // Unlike other channels, this is not a normal message activity so requires special handling.
            if (this.IsTeamsOAuthInvokeActivity(turnContext))
            {
                this.logger.LogInformation($"Running dialog {typeof(T)} with Teams SignIn Invoke Activity.");
                await this.RunDialog(turnContext, cancellationToken);
            }
        }

        /// <summary>
        /// welcome card attachment. If it's null then mo welcome card will be sent.
        /// </summary>
        protected virtual Task<Attachment> GetWelcomeCardAttachmentAsync()
        {
            return null;
        }

        /// <summary>
        /// Is the current activity a Teams Signin Invoke activity (for OAuthPrompt)
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <returns>true if this is a invoke activity from Teams</returns>
        protected bool IsTeamsOAuthInvokeActivity(ITurnContext turnContext)
        {
            return turnContext?.Activity?.Type == ActivityTypes.Invoke &&
                   turnContext?.Activity?.ChannelId == TeamsChannelId &&
                   turnContext?.Activity?.Name == TeamsSignInActivityName;
        }

        /// <summary>
        /// Message activity handler
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            this.logger.LogInformation($"Run dialog {typeof(T).Name} with Message Activity.");
            await this.RunDialog(turnContext, cancellationToken);
        }

        /// <summary>
        /// token response event handler - for scenarios using OAuthPrompts
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new token response Activity.
            this.logger.LogInformation($"Running {typeof(T)} dialog with token response.");
            await this.RunDialog(turnContext, cancellationToken);
        }

        /// <summary>
        /// On member added handler. Used to show welcome card if welcome attachment is not null.
        /// </summary>
        /// <param name="membersAdded"></param>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            // Send welcome card to new users if it's configured
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var attachment = await this.GetWelcomeCardAttachmentAsync();
                    if (attachment != null)
                    {
                        this.logger.LogTrace("Sending welcome card to recipient {0}", member.Id);
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// Run (start/resume) the dialog
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>task</returns>
        protected async Task RunDialog(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await this.dialog.RunAsync(turnContext, this.dialogStateAccessors.DialogStateAccessor, cancellationToken);
        }
    }
}
