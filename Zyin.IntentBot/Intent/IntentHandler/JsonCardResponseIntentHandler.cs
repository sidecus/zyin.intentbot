namespace Zyin.IntentBot.Intent
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Zyin.IntentBot.Services;

    /// <summary>
    /// Simple handler to provide a JSON based adaptive card as response
    /// </summary>
    public abstract class JsonCardResponseIntentHandler<T> : IntentHandler<T>
        where T : IntentContext
    {
        /// <summary>
        /// file content service
        /// </summary>
        private readonly JsonAdaptiveCardService jsonCardService;

        /// <summary>
        /// Initializes a new instance of the JsonAdaptiveCardService
        /// </summary>
        /// <param name="jsonCardService"></param>
        public JsonCardResponseIntentHandler(JsonAdaptiveCardService jsonCardService)
        {
            this.jsonCardService = jsonCardService ?? throw new ArgumentNullException(nameof(jsonCardService));
        }

        /// <summary>
        /// Path to the card JSON file - use string array for x-platform capability
        /// </summary>
        protected abstract string[] CardPaths { get; }

        /// <summary>
        /// Process the intent and send the adaptive card attachment
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="intentContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ProcessIntentInternalAsync(
            ITurnContext turnContext,
            T intentContext,
            CancellationToken cancellationToken)
        {
            // Send the card as attachment
            var jsonCard = await this.jsonCardService.GetJsonAdaptiveCardAsync(this.CardPaths ?? throw new ArgumentNullException(nameof(this.CardPaths)));
            var attachment = jsonCard.GetAttachment();
            await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
        }
    }
}