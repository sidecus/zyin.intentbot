namespace sample.Areas
{
    using System.Threading;
    using System.Threading.Tasks;
    using Zyin.IntentBot.Services;
    using Microsoft.Bot.Builder;

    /// <summary>
    /// Sample intent service which needs to be injected to detect intents
    /// </summary>
    public class SampleIntentService : IIntentService
    {
        public static readonly string Intent_Greetings = "greetings";
        public static readonly string Intent_Sum = "sum";
        public static readonly string Intent_BookingFlight = "bookFlight";
        public static readonly string Intent_MemorizedSum = "previousSumResult";
        public static readonly string Intent_WhoAmI = "whoAmI";

        /// <summary>
        /// Run a query to get the intent string
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Non empty string for known intents, null for unknown intents</returns>
        public Task<string> RunQuery(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var query = turnContext.Activity.AsMessageActivity().Text.ToLowerInvariant();

            switch (query)
            {
                case "hi":
                case "hello":
                    return Task.FromResult(Intent_Greetings);
                case "sum":
                    return Task.FromResult(Intent_Sum);
                case "memory":
                    return Task.FromResult(Intent_MemorizedSum);
                case "flight":
                    return Task.FromResult(Intent_BookingFlight);
                case "who am i":
                case "whoami":
                    return Task.FromResult(Intent_WhoAmI);
                default:
                    return Task.FromResult<string>(null);
            }
        }
    }
}