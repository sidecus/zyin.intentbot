namespace sample.Areas
{
    using System.Threading;
    using System.Threading.Tasks;
    using Zyin.IntentBot.Intent;
    using Microsoft.Bot.Builder;

    /// <summary>
    /// Intent context for greetings
    /// </summary>
    public class GreetingsContext : IntentContext
    {
        /// <summary>
        /// Gets the intent name
        /// </summary>
        public override string IntentName => SampleIntentService.Intent_Greetings;
    }
    
    /// <summary>
    /// Handler to process greetings intent
    /// </summary>
    public class GreetingsIntentHandler : IntentHandler<GreetingsContext>
    {
        /// <summary>
        /// Process the greetings request
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="intentContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ProcessIntentInternalAsync(
            ITurnContext turnContext,
            GreetingsContext intentContext,
            CancellationToken cancellationToken)
        {
            var msg = "Hi there. What can I do for you?";
            await turnContext.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
        }
    }
}