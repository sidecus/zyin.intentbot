namespace Zyin.IntentBot.AdaptiveCard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using AdaptiveCards;

    /// <summary>
    /// Class to create an adaptive card attachment from json format, and perform paremeter formatting
    /// </summary>
    public class JsonAdaptiveCard
    {
        /// <summary>
        /// adaptive card json content
        /// </summary>
        private readonly string jsonContent;

        /// <summary>
        /// Initializes a new instance of the AdaptiveCardAttachment class
        /// </summary>
        /// <param name="jsonCardPath">path to the card json file</param>
        public JsonAdaptiveCard(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentNullException(nameof(jsonContent));
            }

            this.jsonContent = jsonContent;
        }

        /// <summary>
        /// Get an adaptive card attachment with given content. It takes a Dictionary<string, string>.
        /// For each KeyValuePair in the dictionary, we'll replace "{key}" with "value" in the json string.
        /// </summary>
        /// <param name="args">content to be put into the card</param>
        /// <returns>formatted adaptive card attachment with proper values injected</returns>
        public Attachment GetAttachment(Dictionary<string, string> values = null)
        {
            var formattedCard = this.jsonContent;

            if (values?.Any() == true)
            {
                // Format the objects into the card
                var sb = new StringBuilder(formattedCard);
                foreach (var kvp in values)
                {
                    // Replace {name} with value
                    sb.Replace($"{{{kvp.Key}}}", HttpUtility.JavaScriptStringEncode(kvp.Value));
                }

                formattedCard = sb.ToString();
            }

            // Construct attachment object
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject<AdaptiveCard>(formattedCard),
            };
        }
    }
}