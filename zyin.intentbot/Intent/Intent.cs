namespace Zyin.IntentBot.Intent
{
    using System;
    using Zyin.IntentBot.Dialog;

    /// <summary>
    /// Wraps the mapping between an intent and the context object to be collected from a dialog
    /// </summary>
    public interface IIntent
    {
        /// <summary>
        /// intent name
        /// </summary>
        string IntentName { get; }

        /// <summary>
        /// Gets whether the intent requires a dialog to collect more information.
        /// </summary>
        bool HasDialog { get; }

        /// <summary>
        /// Dialog id
        /// </summary>
        string DialogId { get; }

        /// <summary>
        /// Creates the dialog to be used to collect the context info
        /// </summary>
        /// <returns>dialog object</returns>
        InterruptableDialog Dialog { get; }

        /// <summary>
        /// Is the current Intent a fallback intent
        /// </summary>
        bool IsFallbackIntent { get; }

        /// <summary>
        /// Check whether the given intent string matches this intent
        /// </summary>
        /// <param name="intent">intent string</param>
        /// <returns>true if it's a match</returns>
        bool IsMatching(string intent);

        /// <summary>
        /// Creates an context object to collect more information
        /// </summary>
        /// <param name="query">raw user query</param>
        /// <returns>intent context</returns>
        IntentContext CreateIntentContext(string query);
    }
    
    /// <summary>
    /// Intent to handle known intents
    /// </summary>
    /// <typeparam name="TContext">Intent context type</typeparam>
    public class Intent<TContext> : IIntent
        where TContext : IntentContext
    {
        /// <summary>
        /// Whether the intent is a fallback intent.
        /// </summary>
        private static readonly bool IsFallback = typeof(TContext).IsSubclassOf(typeof(BaseFallbackContext));

        /// <summary>
        /// service provider
        /// </summary>
        protected IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new intent which requires user input
        /// </summary>
        /// <param name="intentName">intent name</param>
        /// <param name="serviceProvider">service provider</param>
        public Intent(IServiceProvider serviceProvider)
        {
            // Capture service provider
            this.serviceProvider = serviceProvider;

            // Get a temporary context instance to retrieve the intent name
            var tempContext = this.serviceProvider.GetService(typeof(TContext)) as TContext;
            if (string.IsNullOrWhiteSpace(tempContext?.IntentName))
            {
                throw new ArgumentNullException($"{typeof(TContext).Name} doesn't have valid intent name defined");
            }

            // Set intent name
            this.IntentName = tempContext.IntentName;
        }

        /// <summary>
        /// intent name
        /// </summary>
        public string IntentName { get; private set; }

        /// <summary>
        /// Is the current Intent a fallback intent by checking whether TContext is a subclass of FallbackContext
        /// This is the only correct way to compare for subclassing correctly.
        /// </summary>
        public bool IsFallbackIntent => IsFallback;

        /// <summary>
        /// By default there is no dialog associated.
        /// </summary>
        public bool HasDialog => this.DialogId != null;

        /// <summary>
        /// dialog id, not used by default
        /// </summary>
        public string DialogId => this.Dialog?.Id;

        /// <summary>
        /// handling dialog, not used by default
        /// </summary>
        public virtual InterruptableDialog Dialog => null;

        /// <summary>
        /// Check whether the given intent string matches current intent
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public virtual bool IsMatching(string intent) =>
            string.Equals(this.IntentName, intent, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Creates the context object to collect more information
        /// </summary>
        /// <param name="query">raw user query</param>
        /// <returns>intent context</returns>
        public IntentContext CreateIntentContext(string query)
        {
            var context = this.serviceProvider.GetService(typeof(TContext)) as TContext;
            context.DialogId = this.DialogId;
            context.Query = query ?? throw new ArgumentNullException(nameof(query));

            return context;
        }
    }
}
