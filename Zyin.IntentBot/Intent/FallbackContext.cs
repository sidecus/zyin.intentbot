namespace Zyin.IntentBot.Intent
{
    /// <summary>
    /// Abstract Intent context for fallback which doesn't require authentication
    /// </summary>
    public abstract class AbstractFallbackContext: IntentContext
    {
        /// <summary>
        /// Gets the intent name
        /// </summary>
        public override string IntentName => "fallbackIntent";
    }

    /// <summary>
    /// Intent context for fallback which doesn't require authentication
    /// </summary>
    public sealed class FallbackContext: AbstractFallbackContext
    {
    }

    /// <summary>
    /// Intent context for fallback which requires authentication.
    /// This must inherit from FallbackContext
    /// </summary>
    public sealed class AuthFallbackContext: AbstractFallbackContext
    {
        /// <summary>
        /// Requires authentication
        /// </summary>
        public override bool RequireAuth => true;
    }
}
