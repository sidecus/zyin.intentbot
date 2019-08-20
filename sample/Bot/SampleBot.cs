namespace sample.Bot
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Bot.Schema;
    using Zyin.IntentBot.Bot;
    using Zyin.IntentBot.Dialog;
    using Zyin.IntentBot.Services;
    using System.Threading.Tasks;

    /// <summary>
    /// Main bot
    /// </summary>
    public class SampleBot : DialogBot<IntentDialog>
    {
        /// <summary>
        /// path to the adaptive card json for welcoming purpose
        /// </summary>
        private static readonly string[] cardPaths = { ".", "Bot", "welcomeCard.json" };

        /// <summary>
        /// file content service
        /// </summary>
        private readonly JsonAdaptiveCardService cardService;

        /// <summary>
        /// Constructs a new instance of SampleBot
        /// </summary>
        /// <param name="conversationState"></param>
        /// <param name="userState"></param>
        /// <param name="dialog"></param>
        /// <param name="cardService"></param>
        /// <param name="logger"></param>
        public SampleBot(DialogStateAccessors dialogStateAccessors, IntentDialog dialog, JsonAdaptiveCardService cardService, ILogger<SampleBot> logger)
            : base(dialogStateAccessors, dialog, logger)
        {
            this.cardService = cardService;
        }
        
        /// <summary>
        /// Gets a welcome attachment (overriden) to show the welcome card.
        /// We don't construct this in the constructor since Bot is transient - doing so will cause unintended deserialization.
        /// Putting it direclty in the getter will delay the execution to when it's needed.
        /// </summary>
        protected override async Task<Attachment> GetWelcomeCardAttachmentAsync()
        {
            var jsonAdaptiveCard = await this.cardService.GetJsonAdaptiveCardAsync(cardPaths);
            return jsonAdaptiveCard.GetAttachment();
        }
    }
}

