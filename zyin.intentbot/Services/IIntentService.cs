namespace Zyin.IntentBot.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;

    /// <summary>
    /// Intent service interface
    /// </summary>
    public interface IIntentService
    {
        /// <summary>
        /// Run a query to get the intent string
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>Non empty string for known intents, null for unknown intents</returns>
        Task<string> RunQuery(ITurnContext turnContext, CancellationToken cancellationToken);
    }
}