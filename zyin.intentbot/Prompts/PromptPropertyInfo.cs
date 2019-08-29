namespace Zyin.IntentBot.Prompt
{
    using System;
    using System.Reflection;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    /// <summary>
    /// Class representing a property of intent context to prompt the user in one dialog turn.
    /// </summary>
    public class PromptPropertyInfo
    {
        /// <summary>
        /// Constructs a new PromptPropertyInfo with given PropertyInfo and PromptPropertyAttribute
        /// </summary>
        /// <param name="prop">PropertyInfo object</param>
        /// <param name="promptAttribute">PromptPropertyAttribute associated with the property</param>
        public PromptPropertyInfo(PropertyInfo prop, PromptPropertyAttribute promptAttribute)
            : this(prop.Name,
                    prop.PropertyType,
                    prop.SetValue,
                    prop.GetValue,
                    promptAttribute.Prompt,
                    promptAttribute.Reprompt,
                    promptAttribute.PromptProviderType,
                    promptAttribute.Order)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PromptPropertyInfo class.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType">property type</param>
        /// <param name="setter"></param>
        /// <param name="getter"></param>
        /// <param name="prompt"></param>
        /// <param name="reprompt"></param>
        /// <param name="promptProviderType">prompt provider type</param>
        /// <param name="promptOrder"></param>
        public PromptPropertyInfo(
            string propertyName,
            Type propertyType,
            Action<object, object> setter,
            Func<object, object> getter,
            string prompt,
            string reprompt,
            Type promptProviderType,
            int promptOrder)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            if (setter == null)
            {
                throw new ArgumentNullException(nameof(setter));
            }

            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter));
            }

            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
            this.Setter = setter;
            this.Getter = getter;
            this.Order = promptOrder;

            // Set prompt options
            this.PromptOptions = this.GetPromptOptions(prompt, reprompt);

            // Set the prompt dialog using the given prompt provider type (if it's null we'll generate a default provider)
            this.PromptDialog = this.GetPromptDialog(promptProviderType);
        }

        /// <summary>
        /// Gets the property name
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Property type
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// Setter to set a value into the current intent context
        /// </summary>
        public Action<object, object> Setter { get; private set; }

        /// <summary>
        /// Getter to get the value from the current intent context
        /// </summary>
        public Func<object, object> Getter { get; private set; }

        /// <summary>
        /// Gets the property prompt order
        /// </summary>
        /// <value></value>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the property prompt
        /// </summary>
        public PromptOptions PromptOptions { get; private set; }

        /// <summary>
        /// Gets the prompt dialog used by this property
        /// </summary>
        public Dialog PromptDialog { get; private set; }

        /// <summary>
        /// Get a dialog to use to prompt for this property
        /// </summary>
        private Dialog GetPromptDialog(Type promptProviderType)
        {
            var promptDialogId = $"Prompt_{this.PropertyName}";
            return this.GetPromptProvider(promptProviderType, this.PropertyType).GetPrompt(promptDialogId);
        }

        /// <summary>
        /// Get prompt options. If reprompt is not set, default it to prompt
        /// </summary>
        /// <returns>prompt options</returns>
        private PromptOptions GetPromptOptions(string prompt, string reprompt)
        {
            if (string.IsNullOrWhiteSpace(reprompt))
            {
                reprompt = prompt;
            }

            return new PromptOptions()
            {
                Prompt = MessageFactory.Text(prompt),
                RetryPrompt = MessageFactory.Text(reprompt),
            };
        }

        /// <summary>
        /// Get a prompt provider instance for the property
        /// </summary>
        /// <param name="promptProviderType"></param>
        /// <returns>an IPromptProvider instance</returns>
        private IPromptProvider GetPromptProvider(Type promptProviderType, Type propertyType)
        {
            // Assign a default promptProviderType based on property type if it's not set
            promptProviderType = promptProviderType ?? typeof(TypedPromptProvider<>).MakeGenericType(propertyType);

            // Create prompt provider instance.
            // This requires the provider must have a parameterless constructor.
            return (IPromptProvider)Activator.CreateInstance(promptProviderType);
        }
    }
}