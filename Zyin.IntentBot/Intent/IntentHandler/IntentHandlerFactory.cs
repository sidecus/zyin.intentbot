namespace Zyin.IntentBot.Intent
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Intent handler factory
    /// </summary>
    public class IntentHandlerFactory
    {
        /// <summary>
        /// service provider
        /// </summary>
        protected IServiceProvider serviceProvider;

        /// <summary>
        /// logger
        /// </summary>
        protected ILogger logger;

        /// <summary>
        /// Initializes a new instance of the IntentHandlerFactory
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        public IntentHandlerFactory(IServiceProvider serviceProvider, ILogger<IntentHandlerFactory> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        /// <summary>
        /// Process the given intent context
        /// </summary>
        /// <param name="intentContext">intent context</param>
        /// <param name="cancellationToken"></param>
        /// <returns>task</returns>
        public async Task ProcessIntentAsync(ITurnContext turnContext, IntentContext intentContext, CancellationToken cancellationToken)
        {
            // NOTE - we use service provider and reflection to dynamically locate the intent handler - which can be scoped or transient
            // and depend on other scoped/transient services.
            // We cannot direclty resolve them since IntentFactory and Intents are singletons.
            var intentContextType = intentContext.GetType();
            var handlerGenericType = typeof(IntentHandler<>);
            var handlerType = handlerGenericType.MakeGenericType(intentContextType);
            var handler = (IIntentHandler)this.serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException($"no handler for intentContext {intentContextType.Name}");
            }

            // handle the given intent context
            await handler.ProcessIntentAsync(turnContext, intentContext, cancellationToken);
        }
    }
}