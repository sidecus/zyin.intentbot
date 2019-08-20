namespace sample.Areas
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Zyin.IntentBot.Intent;
    using Zyin.IntentBot.Prompt;
    using Zyin.IntentBot.Bot;
    using sample.Bot;

    /// <summary>
    /// Intent context for adding two numbers
    /// </summary>
    public class AddNumberIntentContext : IntentContext
    {
        /// <summary>
        /// First prompt, number, must be between 1 and 10.
        /// Note we need to use nullable here.
        /// </summary>
        [PromptProperty("What's the first value (between 1 and 10)?", "Value should be between 1 and 10", promptProvider:typeof(SampleNumberPromptProvider), order: 0)]
        public int? First { get; set; }

        /// <summary>
        /// Second prompt, number as well, Int32 no other limitation
        /// Note we need to use nullable here.
        /// </summary>
        [PromptProperty("What's the second value (any int32)?", order: 1)]
        public int? Second { get; set; }

        /// <summary>
        /// Second prompt, number as well, Int32 no other limitation
        /// Note we need to use nullable here.
        /// </summary>
        [PromptProperty("Memorize result?", order: 2)]
        public bool? MemorizeResult { get; set; }

        /// <summary>
        /// Gets the intent name
        /// </summary>
        public override string IntentName => SampleIntentService.Intent_Sum;
    }

    /// <summary>
    /// Intent handler for add number
    /// </summary>
    public class AddNumberIntentHandler : IntentHandler<AddNumberIntentContext>
    {
        /// <summary>
        /// User state accessor
        /// </summary>
        private UserStateAccessors<SampleUserInfo> userStateAccessors;

        /// <summary>
        /// Initialize a new instance of the AddNumberIntentHandler
        /// </summary>
        /// <param name="userStateAccessors"></param>
        public AddNumberIntentHandler(UserStateAccessors<SampleUserInfo> userStateAccessors)
        {
            this.userStateAccessors = userStateAccessors;
        }
        
        /// <summary>
        /// Process the add number intent (after user input has been collected)
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="intentContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ProcessIntentInternalAsync(ITurnContext turnContext, AddNumberIntentContext intentContext, CancellationToken cancellationToken)
        {
            // Perform the sum task
            var x = intentContext.First.Value;
            var y = intentContext.Second.Value;
            var result = x + y;

            if (intentContext.MemorizeResult.Value)
            {
                // Save result to user state since user wants to memorize it
                var userInfo = await this.userStateAccessors.UserInfoAccessor.GetAsync(turnContext, () => new SampleUserInfo());
                userInfo.PreviousSumbResult = result;
                await this.userStateAccessors.UserInfoAccessor.SetAsync(turnContext, userInfo);
            }

            // Output result
            var message = $"{x} + {y} = {result}";
            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }
    }
}