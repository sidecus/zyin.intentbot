namespace Zyin.IntentBot.Intent
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;

    /// <summary>
    /// Default fall back handler which just echos back the query.
    /// </summary>
    public class DefaultFallbackIntentHandler : IntentHandler<FallbackContext>
    {
        /// <summary>
        /// Process the fallback context
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
            var result = $"Default fallback handling query: {intentContext.Query}";
            await turnContext.SendActivityAsync(MessageFactory.Text(result), cancellationToken);
        }
   }
}