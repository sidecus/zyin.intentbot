namespace Zyin.IntentBot.Prompt
{
    using System;

    /// <summary>
    /// Attribute to denote a property which needs prompt and user input
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PromptPropertyAttribute : Attribute
    {
        /// <summary>
        /// prompt string
        /// </summary>
        public string Prompt { get; private set; }

        /// <summary>
        /// reprompt string. If not set, it defaults to the prompt.
        /// </summary>
        public string Reprompt { get; private set; }

        /// <summary>
        /// Gets prompt provider. Can be null.
        /// </summary>
        public Type PromptProviderType { get; private set; }

        /// <summary>
        /// prompt order
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Initializes a new instance of the PromptPropertyAttribute class
        /// </summary>
        /// <param name="prompt">prompt text</param>
        /// /// <param name="retryPrompt">reprompt text</param>
        /// <param name="promptProvider">prompt provider for type check and validation</param>
        /// <param name="order">prompt order</param>
        public PromptPropertyAttribute(string prompt, string retryPrompt = null, Type promptProvider = null, int order = 0)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentNullException(prompt);
            }

            if (promptProvider != null)
            {
                // Prompt provider type must implement IPromptProvider
                if (!(typeof(IPromptProvider).IsAssignableFrom(promptProvider)))
                {
                    throw new ArgumentOutOfRangeException(nameof(promptProvider));
                }
            }

            this.Prompt = prompt;
            this.Reprompt = retryPrompt;
            this.Order = order;
            this.PromptProviderType = promptProvider;
        }
    }
}