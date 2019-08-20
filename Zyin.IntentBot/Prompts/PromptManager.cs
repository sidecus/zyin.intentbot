namespace Zyin.IntentBot.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.Bot.Builder.Dialogs;
    using Zyin.IntentBot.Intent;

    /// <summary>
    /// Factory inteface to process prompt properties
    /// </summary>
    public interface IPromptManager
    {
        /// <summary>
        /// Get prompt dialogs needed by this context
        /// </summary>
        /// <returns>list of prompt dialogs required by this context</returns>
        IEnumerable<Dialog> Dialogs { get; }

        /// <summary>
        /// Get prompt waterfall steps for the given intent context
        /// </summary>
        /// <typeparam name="TContext">Intent context type</typeparam>
        /// <returns>water fall steps for the given TContext</returns>
        IEnumerable<WaterfallStep> WaterfallSteps { get; }
    }

    /// <summary>
    /// Manager class to process prompt related things
    /// </summary>
    public class PromptManager<TContext> : IPromptManager
        where TContext : IntentContext
    {
        /// <summary>
        /// Prompt properties
        /// </summary>
        private readonly IEnumerable<PromptPropertyInfo> promptProperties;

        /// <summary>
        /// Prompt waterfall steps for the properties
        /// </summary>
        private readonly IEnumerable<WaterfallStep> promptWaterfallSteps;

        /// <summary>
        /// Initializes a new instance of the PromptProcessor class
        /// </summary>
        public PromptManager()
        {
            this.promptProperties = this.GetPromptProperties();
            this.promptWaterfallSteps = this.GetPromptWaterfallSteps();
        }

        /// <summary>
        /// Get prompt dialogs needed by this context
        /// </summary>
        /// <returns>list of prompt dialogs required by this context</returns>
        public IEnumerable<Dialog> Dialogs => this.promptProperties.Select(x => x.PromptDialog);

        /// <summary>
        /// Get prompt waterfall steps for the given intent context
        /// </summary>
        /// <typeparam name="TContext">Intent context type</typeparam>
        /// <returns>water fall steps for the given TContext</returns>
        public IEnumerable<WaterfallStep> WaterfallSteps => this.promptWaterfallSteps;

        /// <summary>
        /// Get prompt properties for the current intent context class.
        /// This should be called carefully (best case, only once) since it uses reflection.
        /// </summary>
        /// <returns>prompt properties</returns>
        private IEnumerable<PromptPropertyInfo> GetPromptProperties()
        {
            var info = new List<PromptPropertyInfo>();
            var properties = typeof(TContext).GetProperties();

            foreach (var prop in properties)
            {
                var promptAttribute = prop.GetCustomAttributes(true).OfType<PromptPropertyAttribute>().FirstOrDefault();
                if (promptAttribute == null)
                {
                    // Not a property which needs prompting, skip
                    continue;
                }

                // Add prompt property
                info.Add(new PromptPropertyInfo(prop, promptAttribute));
            }

            if (!info.Any())
            {
                throw new InvalidOperationException($"No info requires user input in {typeof(TContext).Name}");
            }

            // sort and return
            return info.OrderBy(x => x.Order).ToList();
        }

        /// <summary>
        /// Get prompt waterfall steps for info collection
        /// </summary>
        /// <param name="info">property info</param>
        /// <returns>List of waterfall steps for property collection</returns>
        private IEnumerable<WaterfallStep> GetPromptWaterfallSteps()
        {
            var steps = new List<WaterfallStep>();

            // Add steps to get properties and set previous one
            PromptPropertyInfo previousProp = null;
            foreach (var prop in promptProperties)
            {
                steps.Add(this.BuildWaterfallStep(previousProp, prop));
                previousProp = prop;
            }

            // Add step to set last property
            steps.Add(this.BuildWaterfallStep(previousProp, null));
            
            return steps;
        }

         /// <summary>
        /// Get a WaterfallStep given previous property and current property which need to be collected.
        /// The waterfall step will record previous property value and prompt for new property value when needed.
        /// NOTE - we need this function since we need eager evaluation of previousProp and currentProp.
        /// </summary>
        /// <param name="previousProp">previous property which has already been prompted. null means current prop is the first one</param>
        /// <param name="currentProp">current property which needs to be prompted. null means no more.</param>
        /// <returns>A waterstep delegate which does the real work</returns>
        private WaterfallStep BuildWaterfallStep(PromptPropertyInfo previousProp, PromptPropertyInfo currentProp)
        {
            return async (WaterfallStepContext stepContext, CancellationToken cancellationToken) =>
            {
                var intentContext = stepContext.Options as TContext;

                // Try to save the value collected from previous step
                if (previousProp?.Setter != null)
                {
                    // We want the property to be nullable so that we can check whether it's set or not.
                    // However, we cannot convert stepContext.Result of type object with int value to int? using Convert.ChangeType.
                    // So we try to convert stepContext.Result to the underlying non nullable type first.
                    // The Setter will handle non-nullable to nullable conversion automatically.
                    var targetNonNullableType = Nullable.GetUnderlyingType(previousProp.PropertyType) ?? previousProp.PropertyType;
                    var convertedValue = Convert.ChangeType(stepContext.Result, targetNonNullableType);
                    previousProp.Setter(intentContext, convertedValue);
                }

                object currentPropertyValue = null;
                if (currentProp?.Getter != null)
                {
                    // Collect info for current property.
                    // We do null check to see whether the value has been set or not.
                    // This means that the corresponding IntentContext property needs to be "nullable".
                    currentPropertyValue = currentProp.Getter(intentContext);
                    if (currentPropertyValue == null)
                    {
                        // current property is not collected yet. Prompt and collect
                        return await stepContext.PromptAsync(currentProp.PromptDialog.Id, currentProp.PromptOptions, cancellationToken);
                    }
                }

                // If we reach here, it's either no new info needs to be collected (currentGetter == null),
                // or current property is already set (currentProperty != null).
                // Move to next step and pass the currentProperty value.
                return await stepContext.NextAsync(currentPropertyValue, cancellationToken);
            };
        }
   }
}