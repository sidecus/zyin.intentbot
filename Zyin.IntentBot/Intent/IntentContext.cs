namespace Zyin.IntentBot.Intent
{
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Base class for known intent context.
    /// Intent context is used to collect more info, and pass other core info across dialogs e.g. auth token
    /// </summary>
    public abstract class IntentContext
    {
        /// <summary>
        /// Dialog id if this context requires a dialog to collect more info. null if there is no
        /// </summary>
        public string DialogId { get; set; }

        /// <summary>
        /// The user query
        /// </summary>
        /// <value>gets the user query</value>
        public string Query { get; set; }

        /// <summary>
        /// Does the context require dialog?
        /// </summary>
        public bool HasDialog => !string.IsNullOrWhiteSpace(this.DialogId);

        /// <summary>
        /// Does this intent context require authentication?
        /// </summary>
        public virtual bool RequireAuth => false;

        /// <summary>
        /// Get confirmation prompt string. If this returns null, we'll continue silently with the action
        /// </summary>
        public virtual string ConfirmationString => null;

        /// <summary>
        /// Intent name
        /// </summary>
        public abstract string IntentName { get; }
    }

    /// <summary>
    /// Base class for known intent context which requires auth.
    /// Intent context is used to collect more info, and pass other core info across dialogs e.g. auth token
    /// </summary>
    public abstract class AuthIntentContext : IntentContext
    {
        /// <summary>
        /// Does this intent context require authentication?
        /// </summary>
        public override bool RequireAuth => true;
    }
}