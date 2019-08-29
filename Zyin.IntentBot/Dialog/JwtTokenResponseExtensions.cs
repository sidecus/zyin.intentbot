namespace Zyin.IntentBot.Dialog
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// extension method for TokenResponse
    /// </summary>
    public static class JwtTokenResponseExtensions
    {
        /// <summary>
        /// Check whether token is still valid
        /// </summary>
        /// <param name="tokenRespose">bot token response</param>
        public static bool IsTokenValid(this TokenResponse tokenResponse)
        {
            var response = tokenResponse ?? throw new ArgumentNullException(nameof(tokenResponse));

            var token = response.Token;
            if (token == null)
            {
                return false;
            }

            var jwtToken = new JwtSecurityToken(token);
            return jwtToken.ValidTo > DateTime.UtcNow.AddMinutes(5);
        }
    }
}