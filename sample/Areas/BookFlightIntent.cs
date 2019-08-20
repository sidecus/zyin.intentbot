namespace sample.Areas
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Zyin.IntentBot.Intent;
    using Zyin.IntentBot.Prompt;

    /// <summary>
    /// Intent context for booking a flight
    /// </summary>
    public class BookFlightIntentContext : IntentContext
    {
        /// <summary>
        /// To which city
        /// </summary>
        [PromptProperty("What's your destination city?", order: 0)]
        public string To { get; set; }

        /// <summary>
        /// From which city
        /// </summary>
        [PromptProperty("From which city?", order: 1)]
        public string From { get; set; }

        /// <summary>
        /// Second prompt, number as well, Int32 no other limitation
        /// Note we need to use nullable here.
        /// </summary>
        [PromptProperty("When do you want to fly", retryPrompt: "Please be a bit more specific about the date", order: 2)]
        public DateTime? When { get; set; }

        /// <summary>
        /// Set this to true to require the OAuth prompt
        /// </summary>
        public override bool RequireAuth => false;

        /// <summary>
        /// Gets the intent name
        /// </summary>
        public override string IntentName => SampleIntentService.Intent_BookingFlight;
    }

    public class BookFlightIntentHandler : IntentHandler<BookFlightIntentContext>
    {
        protected override async Task ProcessIntentInternalAsync(ITurnContext turnContext, BookFlightIntentContext intentContext, CancellationToken cancellationToken)
        {
            // Output result
            var message = $"Booking a filght from {intentContext.From} to {intentContext.To} on {intentContext.When.Value.ToShortDateString()}.";
            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }
    }
}