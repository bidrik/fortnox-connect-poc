using System;
using Newtonsoft.Json;

namespace FortnoxConnect.Models
{
    /// <summary>
    /// Model for OAuth2 token response from Fortnox
    /// </summary>
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// Calculated expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
