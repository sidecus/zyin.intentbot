namespace Zyin.IntentBot.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
    using Zyin.IntentBot.Dialog;

    /// <summary>
    /// Date prompt which returns a date
    /// </summary>
    public class DatePrompt : InterruptableDialog
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// </summary>
        /// <param name="id"></param>
        public DatePrompt(string id = null)
            : base(id ?? nameof(DatePrompt))
        {
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), DefiniteTimexValidator));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Initial step to start the date time prompt
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = (PromptOptions)stepContext.Options;

            // We were not given any date at all so prompt the user.
            return await stepContext.PromptAsync(nameof(DateTimePrompt), promptOptions, cancellationToken);
        }

        /// <summary>
        /// Final step to pass the result back.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var timex = ((List<DateTimeResolution>)stepContext.Result)[0].Timex;
            var parsed = new TimexProperty(timex);
            var result = new DateTime(parsed.Year.Value, parsed.Month.Value, parsed.DayOfMonth.Value);

            return await stepContext.EndDialogAsync(result, cancellationToken);
        }

        /// <summary>
        /// Validate to make sure the timex is definite
        /// </summary>
        /// <param name="promptContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>valid or not</returns>
        private static Task<bool> DefiniteTimexValidator(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                var timex = promptContext.Recognized.Value[0].Timex.Split('T')[0];

                // If this is a definite Date including year, month and day we are good otherwise reprompt.
                // A better solution might be to let the user know what part is actually missing.
                var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Definite);

                return Task.FromResult(isDefinite);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
