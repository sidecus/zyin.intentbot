namespace Zyin.IntentBot.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Bot.Builder.Dialogs;

    /// <summary>
    /// prompt provider interface. Don't implement this directly. Always inherit from TypedPromptProvider.
    /// </summary>
    public interface IPromptProvider
    {
        /// <summary>
        /// Get a Prompt/Dialog object to prompt and validate one user input
        /// </summary>
        /// <param name="promptDialogId">dialog id for this prompt</param>
        /// <returns>Dialog used to prompt (and do custom validation but it's opitional)</returns>
        Dialog GetPrompt(string promptDialogId);
    }

    /// <summary>
    /// Typed prompt provider class
    /// </summary>
    public class TypedPromptProvider<TProperty> : IPromptProvider
    {
        /// <summary>
        /// Default supported prompt func delegates.
        /// </summary>
        protected static readonly Dictionary<Type, Func<string, Dialog>> DefaultTypedPromptFuncMap;

        /// <summary>
        /// Initializes a new instance of the TypedPromptProvider class.
        /// </summary>
        static TypedPromptProvider()
        {
            DefaultTypedPromptFuncMap = new Dictionary<Type, Func<string, Dialog>>()
            {
                { typeof(string),       GetTextPrompt },
                { typeof(int?),         GetIntPrompt },
                { typeof(double?),      GetDoublePrompt },
                { typeof(bool?),        GetBoolPrompt },
                { typeof(DateTime?),    GetDateTimePrompt },
            };

            // We enforce the type check here to ensure all types either class or nullables.
            // The null is required in PromptManager to see whether a property has been set or not.
            // Note: You can implement custom TypedPromptProvider which takes a type other than the ones
            //   supported by default. But it'll fail the below check. This is done by intention since
            //   I'd like to make sure all supported types have a default implementation.
            if (!DefaultTypedPromptFuncMap.ContainsKey(typeof(TProperty)))
            {
                var supportedTypes = string.Join(",", SupportedTypes.Select(t => (Nullable.GetUnderlyingType(t) ?? t).Name));
                var msg = $"{typeof(TProperty).Name} is not supported by TypedPromptProvider. Supported types: {supportedTypes}.";
                throw new NotSupportedException(msg);
            }
        }

        /// <summary>
        /// Gets a list of supported types by TypedPromptProvider.
        /// </summary>
        protected static IEnumerable<Type> SupportedTypes =>
            DefaultTypedPromptFuncMap.Keys
            .Select(t => (Nullable.GetUnderlyingType(t) ?? t))
            .ToList();

        /// <summary>
        /// Get a Prompt dialog object to prompt and validate one user input based on the property type.
        /// </summary>
        /// <param name="promptDialogId">dialog id for this prompt</param>
        /// <returns>Dialog used to prompt (and do custom validation but it's opitional)</returns>
        public virtual Dialog GetPrompt(string promptDialogId)
        {
            if (string.IsNullOrWhiteSpace(promptDialogId))
            {
                throw new ArgumentNullException(nameof(promptDialogId));
            }

            // The type check is done in the static constructor so we don't need to check here again.
            return DefaultTypedPromptFuncMap[typeof(TProperty)](promptDialogId);
        }

        // Default supported prompt dialog generation functions
        private static Func<string, Dialog> GetTextPrompt = promptName => new TextPrompt(promptName) as Dialog;
        private static Func<string, Dialog> GetIntPrompt = promptName => new NumberPrompt<int>(promptName) as Dialog;
        private static Func<string, Dialog> GetDoublePrompt = promptName => new NumberPrompt<double>(promptName) as Dialog;
        private static Func<string, Dialog> GetBoolPrompt = promptName => new ConfirmPrompt(promptName) as Dialog;
        private static Func<string, Dialog> GetDateTimePrompt = promptName => new DatePrompt(promptName) as Dialog;
    }
}