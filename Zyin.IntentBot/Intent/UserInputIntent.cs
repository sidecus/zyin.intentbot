namespace Zyin.IntentBot.Intent
{
    using Zyin.IntentBot.Dialog;

    /// <summary>
    /// Intent which needs more user input to be collected via a dialog
    /// </summary>
    /// <typeparam name="TContext">The context object type which the dialog needs to capture</typeparam>
    public sealed class UserInputIntent<TContext> : Intent<TContext>
        where TContext: IntentContext, new()
    {
        /// <summary>
        /// Initializes a new UserInputIntent with standard UserInputDialog
        /// </summary>
        /// <param name="dialog"></param>
        public UserInputIntent(UserInputDialog<TContext> dialog)
        {
            // Notes - all intents are singleton. So Dialogs should also be registered as singletons.
            // This means it doesn't maintain any transient state by itself!
            this.Dialog = dialog;
        }

        /// <summary>
        /// Dialog used to collect user input
        /// </summary>
        public override InterruptableDialog Dialog { get; }
    }
}