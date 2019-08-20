namespace Zyin.IntentBot.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.AI.Luis;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Zyin.IntentBot.Config;

    /// <summary>
    /// Luis service implementation
    /// </summary>
    public class LuisService : IIntentService
    {
        private readonly LuisRecognizer recognizer;
        private readonly ILogger logger;

        /// <summary>
        /// Luis service using options
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public LuisService(IOptions<LuisConfig> options, ILogger<LuisService> logger)
        {
            // Create the LUIS settings from configuration.
            var luisApplication = new LuisApplication(
                options.Value.LuisAppId,
                options.Value.LuisApiKey,
                options.Value.LuisApiEndPoint
            );
            this.recognizer = new LuisRecognizer(luisApplication);

            // logger
            this.logger = logger;
        }

        /// <summary>
        /// Default score threshold
        /// </summary>
        protected virtual double ScoreThreshold => 0.8;

        /// <summary>
        /// Run a query to get the intent string
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Non empty string for known intents, null for unknown intents</returns>
        public async Task<string> RunQuery(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            try
            {
                // The actual call to LUIS
                var recognizerResult = await this.recognizer.RecognizeAsync(turnContext, cancellationToken);

                // Get top intent and check threshold
                var (intent, score) = recognizerResult.GetTopScoringIntent();
                this.logger.LogInformation($"Luis input: {turnContext.Activity?.AsMessageActivity()?.Text}, intent: {intent}, score: {score}");

                if (score >= this.ScoreThreshold)
                {
                    return intent;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"LUIS Exception: {e.Message} Check your LUIS configuration.");
            }

            return null;
        }
    }
}