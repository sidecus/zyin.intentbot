namespace Zyin.IntentBot.Intent
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Simple handler to provide an image as response
    /// </summary>
    public abstract class ImageResponseIntentHandler<T> : IntentHandler<T>
        where T : IntentContext
    {
        /// <summary>
        /// logger instance
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// static image path
        /// </summary>
        private readonly string baseUrl;

        /// <summary>
        /// Initializes a new instance of the HatersHateIntentHandler class
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public ImageResponseIntentHandler(IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var request = (httpContextAccessor?.HttpContext?.Request) ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.baseUrl = $"{request.Scheme}://{request.Host.Value}/";
        }

        /// <summary>
        /// Image url.
        /// 1. If it's a relative path (e.g. images/thankyou.gif), we'll send by appending website base url.
        /// 2. If it's an absolute path, we'll use it as is.
        /// </summary>
        protected abstract string ImageUrl { get; }

        /// <summary>
        /// Image content type. Defaults to image/gif
        /// </summary>
        protected virtual string ContentType => "image/gif";

        /// <summary>
        /// Check whether the given URL is a relative URL by trying to parse it as Uri
        /// </summary>
        private bool IsRelativeUrl
        {
            get
            {
                try
                {
                    var uri = new Uri(this.ImageUrl ?? throw new ArgumentNullException(nameof(this.ImageUrl)));
                }
                catch (UriFormatException)
                {
                    this.logger.LogTrace("Image url is potentially relative url: {ImageUrl}", this.ImageUrl);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the image full URL to send to client
        /// </summary>
        /// <value></value>
        private string ImageFullUrl
        {
            get
            {
                return this.IsRelativeUrl ? (this.baseUrl + this.ImageUrl.TrimStart('/')) : this.ImageUrl;
            }
        }

        /// <summary>
        /// Process the greetings request
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
            // Workaround the Teams limitation on gif by using HTML
            await turnContext.SendActivityAsync(MessageFactory.Text($"<img src='{this.ImageFullUrl}' height='200'/>"), cancellationToken);

            /*
            var attachment = new Attachment(this.ContentType, this.baseUrl + this.ImageRelativePath);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);
            */
        }
    }
}