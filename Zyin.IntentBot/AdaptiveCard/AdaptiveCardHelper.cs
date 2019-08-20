namespace Zyin.IntentBot.AdaptiveCard
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Newtonsoft.Json;

    /// <summary>
    /// Adaptive image supporting long uri for data URI schema based images.
    /// This is to workaround the issue with default Adaptive Image where it's using Uri as url type, and
    /// Uri has URL length check.
    /// </summary>
    public class AdaptiveImageWithLongUrl : AdaptiveImage
    {
        [JsonProperty(PropertyName = "url", Required = Required.Always)]
        public string LongUrl { get; set; }
    }

    /// <summary>
    /// Adaptive card helper
    /// </summary>
    public static class AdaptiveCardHelper
    {
        /// <summary>
        /// Get an empty adaptive card with header
        /// </summary>
        /// <param name="headerText"></param>
        /// <returns></returns>
        public static AdaptiveCard GetEmptyCardWithHeader(string headerText)
        {
            if (string.IsNullOrWhiteSpace(headerText))
            {
                throw new ArgumentNullException(nameof(headerText));
            }

            return new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Style = AdaptiveContainerStyle.Emphasis,
                        Bleed = true,
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = headerText,
                                Weight = AdaptiveTextWeight.Bolder,
                                Size = AdaptiveTextSize.Large,
                            },
                        }
                    },
                }
            };
        }
    }
}
