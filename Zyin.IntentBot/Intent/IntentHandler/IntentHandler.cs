namespace Zyin.IntentBot.Intent
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;

    /// <summary>
    /// Intent handler base class
    /// </summary>
    public interface IIntentHandler
    {
        /// <summary>
        /// Process the intent based on the intent context
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="intentContext">intent context</param>
        /// <param name="cancellationToken"></param>
        /// <returns>task</returns>
        Task ProcessIntentAsync(ITurnContext turnContext, IntentContext intentContext, CancellationToken cancellationToken);
    }
    
    /// <summary>
    /// Intent handler base class
    /// </summary>
    public abstract class IntentHandler<T> : IIntentHandler
        where T : IntentContext
    {
        /// <summary>
        /// Process the intent based on the intent context
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="intentContext">intent context</param>
        /// <param name="cancellationToken"></param>
        /// <returns>task</returns>
        public Task ProcessIntentAsync(
            ITurnContext turnContext,
            IntentContext intentContext,
            CancellationToken cancellationToken)
        {
            if (!(intentContext is T))
            {
                throw new InvalidOperationException($"intentContext of {intentContext.GetType().Name} is not allowed");
            }

            return this.ProcessIntentInternalAsync(turnContext, intentContext as T, cancellationToken);
        }
        
        /// <summary>
        /// Internal processing of the intent based on the intent context
        /// </summary>
        /// <param name="turnContext">turn context</param>
        /// <param name="intentContext">intent context</param>
        /// <param name="cancellationToken"></param>
        /// <returns>task</returns>
        protected abstract Task ProcessIntentInternalAsync(
            ITurnContext turnContext,
            T intentContext,
            CancellationToken cancellationToken);
    }
}