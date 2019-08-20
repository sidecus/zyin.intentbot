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
    public class MemorizedSumResultIntentContext : IntentContext
    {
        /// <summary>
        /// Gets the intent name
        /// </summary>
        public override string IntentName => SampleIntentService.Intent_MemorizedSum;
    }

    public class MemorizedSumResultIntentHandler : IntentHandler<MemorizedSumResultIntentContext>
    {
        private UserStateAccessors<SampleUserInfo> userStateAccessors;

        public MemorizedSumResultIntentHandler(UserStateAccessors<SampleUserInfo> userStateAccessors)
        {
            this.userStateAccessors = userStateAccessors;
        }
        
        protected override async Task ProcessIntentInternalAsync(ITurnContext turnContext, MemorizedSumResultIntentContext intentContext, CancellationToken cancellationToken)
        {
            // Try to get the previous result
            var userInfo = await this.userStateAccessors.UserInfoAccessor.GetAsync(turnContext);
            var message = userInfo?.PreviousSumbResult == null ?
                "There is nothing saved in memory." :
                $"The previous result is {userInfo.PreviousSumbResult}";

            // Output result
            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }
    }
}