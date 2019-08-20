namespace Zyin.IntentBot.Intent
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Bot.Builder.Dialogs;

    /// <summary>
    /// Intent context factory which manages all intent to context mapping
    /// </summary>
    public interface IIntentFactory
    {
        /// <summary>
        /// Gets list of dialogs needed by this intent
        /// </summary>
        IEnumerable<Dialog> Dialogs { get; }

        /// <summary>
        /// Find the matching intent based on an intent string
        /// </summary>
        /// <param name="intentString">intent string</param>
        /// <param name="query">raw user query</param>
        /// <returns>matching intent context, or null</returns>
        IntentContext CreateIntentContext(string intentString, string query);
    }
    
    /// <summary>
    /// Intent context factory which manages all intent to context mapping
    /// </summary>
    public sealed class IntentFactory : IIntentFactory
    {
        /// <summary>
        /// Collection of known intents
        /// </summary>
        private readonly IEnumerable<IIntent> knownIntents;

        /// <summary>
        /// Fallback intent
        /// </summary>
        private readonly IIntent fallbackIntent;

        /// <summary>
        /// logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Gets the list of dialog objects needed by intent with dialogs.
        /// </summary>
        public IEnumerable<Dialog> Dialogs => this.knownIntents.Where(x => x.HasDialog).Select(x => x.Dialog);

        /// <summary>
        /// Initializes a new instance of the intent factory
        /// </summary>
        /// <param name="intents">intents injected over DI</param>
        /// <param name="logger">logger</param>
        public IntentFactory(IEnumerable<IIntent> intents, ILogger<IntentFactory> logger)
        {
            if (intents == null || intents.Count() == 0)
            {
                throw new ArgumentNullException(nameof(intents));
            }

            this.logger = logger;

            this.knownIntents = intents.Where(x => !x.IsFallbackIntent).ToList();
            this.fallbackIntent = intents.LastOrDefault(x => x.IsFallbackIntent) ?? throw new InvalidOperationException("No fall back intent registered!");
        }

        /// <summary>
        /// Find the matching intent based on an intent string.
        /// </summary>
        /// <param name="intentString">intent string</param>
        /// <param name="query">raw query</param>
        /// <returns>matching intent context, or null</returns>
        public IntentContext CreateIntentContext(string intentString, string query)
        {
            return this.FindIntent(intentString).CreateIntentContext(query);
        }
        
        /// <summary>
        /// Find the matching intent based on an intent string.
        /// </summary>
        /// <param name="intentString">intent string</param>
        /// <returns>matching intent context - never null</returns>
        private IIntent FindIntent(string intentString)
        {
            if (!string.IsNullOrWhiteSpace(intentString))
            {
                foreach(var knownIntent in this.knownIntents)
                {
                    if (knownIntent.IsMatching(intentString))
                    {
                        this.logger.LogInformation($"Found intent {intentString}. dialog is {knownIntent.DialogId}");

                        // Return a known intent context
                        return knownIntent;
                    }
                }
            }

            // No matching intent found. Return fallback intent context
            this.logger.LogWarning($"unknown intent {intentString}. Using fallback intent instead.");
            return this.fallbackIntent;
        }
    }
}