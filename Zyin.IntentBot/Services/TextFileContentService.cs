namespace Zyin.IntentBot.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Zyin.IntentBot.AdaptiveCard;

    /// <summary>
    /// Class which provides json adaptive card file reading
    /// </summary>
    public class JsonAdaptiveCardService
    {
        private readonly string contentRootPath;
        private readonly ConcurrentDictionary<string, JsonAdaptiveCard> knownCards = new ConcurrentDictionary<string, JsonAdaptiveCard>();

        /// <summary>
        /// Initialize a new instance of the ContentTextFileService class
        /// </summary>
        /// <param name="env"></param>
        public JsonAdaptiveCardService(IHostingEnvironment env)
        {
            this.contentRootPath = env.ContentRootPath;
        }

        /// <summary>
        /// Get file content based on relative path to the content root
        /// </summary>
        /// <param name="relativePaths">relative paths to the card json file. This is case sensitive.</param>
        /// <returns></returns>
        public async Task<JsonAdaptiveCard> GetJsonAdaptiveCardAsync(IEnumerable<string> relativePaths)
        {
            if (relativePaths == null || !relativePaths.Any())
            {
                throw new ArgumentNullException(nameof(relativePaths));
            }

            var path = this.GetAbsolutePath(relativePaths);
            if (this.knownCards.ContainsKey(path))
            {
                return this.knownCards[path];
            }
            else
            {
                using (var file = File.OpenText(path))
                {
                    var content = await file.ReadToEndAsync();
                    var card = new JsonAdaptiveCard(content);
                    this.knownCards[path] = card;
                    return card;
                }
            }
        }

        /// <summary>
        /// Get the absolute path for the relative paths to the content root
        /// </summary>
        /// <param name="relativePaths"></param>
        /// <returns></returns>
        private string GetAbsolutePath(IEnumerable<string> relativePaths)
        {
            var pathArray = new List<string>{ this.contentRootPath };
            pathArray.AddRange(relativePaths);

            return Path.Combine(pathArray.ToArray());
        }
    }
}