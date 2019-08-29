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
        private UserStateManager<SampleUserInfo> userInfoManager;

        public MemorizedSumResultIntentHandler(UserStateManager<SampleUserInfo> userInfoManager)
        {
            this.userInfoManager = userInfoManager;
        }
        
        protected override async Task ProcessIntentInternalAsync(ITurnContext turnContext, MemorizedSumResultIntentContext intentContext, CancellationToken cancellationToken)
        {
            // Try to get the previous result
            var userInfo = await this.userInfoManager.GetAsync(turnContext, cancellationToken);
            var message = userInfo?.SavedSumResult == null ?
                "There is nothing saved in memory." :
                $"The previous result is {userInfo.SavedSumResult}";

            // Output result
            await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        }
    }
}