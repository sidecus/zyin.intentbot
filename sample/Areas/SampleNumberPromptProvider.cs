namespace sample.Areas
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Zyin.IntentBot.Prompt;

    /// <summary>
    /// Sample Number prompt with value validation
    /// </summary>
    public class SampleNumberPromptProvider : TypedPromptProvider<int?>
    {
        /// <summary>
        /// Get a Prompt/Dialog object prompts and validates
        /// </summary>
        /// <param name="promptName"></param>
        /// <returns>Dialot used to prompt</returns>
        public override Dialog GetPrompt(string promptName)
        {
            return new NumberPrompt<int>(promptName, this.ValidateNumber);
        }

        /// <summary>
        /// number validator
        /// </summary>
        /// <param name="promptContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private Task<bool> ValidateNumber(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var value = promptContext.Recognized.Value;
                if (value >= 1 && value <= 10)
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);;
        }
    }
}