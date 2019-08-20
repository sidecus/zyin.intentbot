namespace Zyin.IntentBot.Dialog
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Dialog which can be interrupted. Used in subdialogs which are collecting more info for an intent for user to break out if needed.
    /// </summary>
    public class InterruptableDialog : ComponentDialog
    {
        /// <summary>
        /// Constructs a new InterruptableDialog with the given dialog id
        /// </summary>
        /// <param name="id">dialog id</param>
        public InterruptableDialog(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Overriden OnBeginDialogAsync to handle user interruption
        /// </summary>
        /// <param name="innerDc">current context</param>
        /// <param name="options">options</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns>dialog turn result</returns>
        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await this.InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        /// <summary>
        /// Overriden OnContinueDialogAsync to handle user interruption
        /// </summary>
        /// <param name="innerDc">current context</param>
        /// <param name="options">options</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns>dialog turn result</returns>
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var result = await this.InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        /// <summary>
        /// Check user message and decide whether we need to interrupt the current dialog
        /// </summary>
        /// <param name="innerDc">current inner dialog context</param>
        /// <param name="cancellationToken">cancelation token</param>
        /// <returns>dialog turn result</returns>
        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                switch (text)
                {
                    case "bye":
                    case "cancel":
                    case "quit":
                    case "exit":
                    case "start over":
                    case "restart":
                        await innerDc.Context.SendActivityAsync($"Cancelling", cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();
                }
            }

            return null;
        }
    }
}
