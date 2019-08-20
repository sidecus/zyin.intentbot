namespace sample.Areas
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Zyin.IntentBot.Intent;

    /// <summary>
    /// Fallback Handler to process intents which are not known to the bot.
    /// We inherit from IntentHandler<FallbackContext>. In this case fall back processing is anonymous.
    /// If we inherit from IntentHandler<AuthFallbackContext>, then all fallback will require authentication.
    /// </summary>
    public class SampleFallbackHandler : IntentHandler<FallbackContext>
    {
        /// <summary>
        /// Process the fallback context (query)
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="intentContext"></param>
        /// <param name="calcellationToken"></param>
        /// <returns></returns>
        protected override async Task ProcessIntentInternalAsync(
            ITurnContext turnContext,
            FallbackContext intentContext,
            CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text($"Unknown intent handled by fallback handler. Query:{intentContext.Query}"), cancellationToken);
        }
   }
}