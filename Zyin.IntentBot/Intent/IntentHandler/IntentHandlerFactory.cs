namespace Zyin.IntentBot.Intent
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Intent handler factory
    /// </summary>
    public class IntentHandlerFactory
    {
        /// <summary>
        /// IntentContext type to IntentHandler type map
        /// </summary>
        private readonly ConcurrentDictionary<Type, Type> HandlerTypeMap = new ConcurrentDictionary<Type, Type>();

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
        /// Get the intent handler given the intent context
        /// </summary>
        /// <param name="intentContext">intent context</param>
        /// <returns>task</returns>
        public IIntentHandler GetIntentHandler(IntentContext intentContext)
        {
            intentContext = intentContext ?? throw new ArgumentNullException(nameof(intentContext));

            // NOTE - we use service provider and reflection to dynamically locate the intent handler - which can be scoped or transient
            // and depend on other scoped/transient services.
            // We cannot direclty resolve them since intents are singletons.
            var handlerType = this.GetHandlerType(intentContext);
            var handler = (IIntentHandler)this.serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                var error = $"No handler registered for {intentContext.GetType().Name}.";
                this.logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            return handler;
        }

        /// <summary>
        /// Get handler type for the given intent context.
        /// Uses local cache first to avoid unnecessary reflection.
        /// </summary>
        /// <param name="intentContext"></param>
        /// <returns></returns>
        private Type GetHandlerType(IntentContext intentContext)
        {
            var intentContextType = intentContext.GetType();
            var result = this.HandlerTypeMap.GetOrAdd(
                intentContextType,
                x =>
                {
                    // Not in the map before. Use reflection to get it.
                    var handlerGenericType = typeof(IntentHandler<>);
                    var handlerType = handlerGenericType.MakeGenericType(intentContextType);
                    return handlerType;
                });

            return result;
        }
    }
}