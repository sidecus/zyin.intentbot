namespace Zyin.IntentBot.Dialog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Zyin.IntentBot.Intent;
    using Zyin.IntentBot.Prompt;

    /// <summary>
    /// Dialog using waterfall steps to collect more info for the intent context
    /// </summary>
    public class UserInputDialog<TContext> : InterruptableDialog
        where TContext: IntentContext
    {
        /// <summary>
        /// Prompt handler for the given input context
        /// </summary>
        private readonly IPromptManager promptManager;

        /// <summary>
        /// Constructs a new instance of the InfoCollectionDialog class
        /// </summary>
        /// <param name="dialogId">dialog id</param>
        public UserInputDialog(PromptManager<TContext> promptManager)
            : base($"UserInput_{typeof(TContext).Name}")
        {
            // Set prompt Manager
            if (promptManager == null)
            {
                throw new ArgumentNullException(nameof(promptManager));
            }

            this.promptManager = promptManager;

            // Add dialogs required by prompt
            this.AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            this.promptManager.Dialogs.ToList().ForEach(p => this.AddDialog(p));

            // Add waterfall dialog and steps for user input collection as well as confirm step and final step
            var waterfallSteps = new List<WaterfallStep>();
            waterfallSteps.AddRange(this.promptManager.WaterfallSteps);
            waterfallSteps.Add(this.ConfirmStepAsync);
            waterfallSteps.Add(this.FinalStepAsync);
            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // Set the initial child Dialog to run to be the waterfall dialog
            this.InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Prompt the user to confirm whether proceed or not
        /// </summary>
        /// <param name="stepContext">step context</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns>DialogTurnResult</returns>
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // stepContext.Result must be null here. This means there is no new info to be collected
            if (stepContext.Result != null)
            {
                throw new InvalidOperationException("ConfirmStep is invoked with pending info.");
            }

            var intentContext = stepContext.Options as TContext;
            var msg = intentContext.ConfirmationString;

            if (msg != null)
            {
                // Let user confirm
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
            }
            else
            {
                // Go to next step with result set to true (assuming confirmed by default)
                return await stepContext.NextAsync(true, cancellationToken);
            }
        }

        /// <summary>
        /// Check user response on the confirmation and decide whether to continue or abandon dialog
        /// </summary>
        /// <param name="stepContext">step context</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns>DialogTurnResult</returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // End dialog with info collected which gets passed as stepContext.Result in IntentDialog.
            var confirmed = (bool)stepContext.Result;

            // If user confirms, pass collected info back. Otherwise null.
            var intentContext = confirmed ? stepContext.Options as TContext : null;
            return await stepContext.EndDialogAsync(intentContext, cancellationToken);
        }
    }
}
