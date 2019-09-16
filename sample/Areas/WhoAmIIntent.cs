namespace sample.Areas
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Zyin.IntentBot.Intent;
    using Zyin.IntentBot.Services;

    /// <summary>
    /// Intent context for whoami, which will return your user info from AAD graph api.
    /// This is an authenticated context so it'll ask for authentication. Check default.htm for more details.
    /// </summary>
    public class WhoAmIContext : AuthIntentContext
    {
        public override string IntentName => SampleIntentService.Intent_WhoAmI;
    }

    /// <summary>
    /// Handler to process greetings intent
    /// </summary>
    public class WhoAmIIntentHandler : IntentHandler<WhoAmIContext>
    {
        /// <summary>
        /// Path to profile card
        /// </summary>
        private static readonly string[] profileCardPaths = { "Areas", "profileCard.json" };

        /// <summary>
        /// Graph service reference
        /// </summary>
        protected readonly GraphService graphService;

        /// <summary>
        /// file content service
        /// </summary>
        private readonly JsonAdaptiveCardService jsonCardService;

        /// <summary>
        /// Initialize a new instance of the <see cref="WhoAmIIntentHandler"> class
        /// </summary>
        /// <param name="graphService">graph service</param>
        public WhoAmIIntentHandler(GraphService graphService, JsonAdaptiveCardService jsonCardServie)
        {
            this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            this.jsonCardService = jsonCardServie ?? throw new ArgumentNullException(nameof(jsonCardServie));
        }

        /// <summary>
        /// Process the whoami request
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="intentContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task ProcessIntentInternalAsync(
            ITurnContext turnContext,
            WhoAmIContext intentContext,
            CancellationToken cancellationToken)
        {
            // Set user token
            var token = intentContext.AppToken;
            this.graphService.InitializeTokenAsync(token);

            // Get profile and photo
            var profileTask = this.graphService.GetMyProfile();
            var photoTask = this.graphService.GetMyPhoto();
            await Task.WhenAll(profileTask, photoTask);
            var profile = await profileTask;
            var photoUrl = await photoTask;

            var values = new Dictionary<string, string>()
            {
                { nameof(photoUrl), photoUrl },
                { "name", profile.name },
                { "title", profile.title ?? "No Title Specified" },
                { "mail", profile.mail },
                { "mobilePhone", profile.mobilePhone },
                { "id", profile.id.ToString() },
            };

            // Construct profile card and return
            var jsonCard = await this.jsonCardService.GetJsonAdaptiveCardAsync(profileCardPaths);
            var attachment = jsonCard.GetAttachment(values);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
        }
    }
}