using System.Net;
using System.Text;
using FortnoxConnect.Models;
using Newtonsoft.Json;

namespace FortnoxConnect.Services
{
    /// <summary>
    /// Service for handling OAuth2 authentication with Fortnox API
    /// This is a testable version extracted from AuthController
    /// </summary>
    public class FortnoxAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly string _tokenEndpoint;

        public FortnoxAuthService(
            HttpClient httpClient,
            string clientId,
            string clientSecret,
            string redirectUri,
            string tokenEndpoint)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _redirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
            _tokenEndpoint = tokenEndpoint ?? throw new ArgumentNullException(nameof(tokenEndpoint));
        }

        /// <summary>
        /// Exchanges authorization code for access token
        /// </summary>
        public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Authorization code cannot be null or empty", nameof(code));

            var requestBody = string.Format(
                "grant_type=authorization_code&code={0}&redirect_uri={1}&client_id={2}&client_secret={3}",
                Uri.EscapeDataString(code),
                Uri.EscapeDataString(_redirectUri),
                Uri.EscapeDataString(_clientId),
                Uri.EscapeDataString(_clientSecret)
            );

            var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync(_tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Token exchange failed with status {response.StatusCode}: {errorContent}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

            if (tokenResponse == null)
                throw new InvalidOperationException("Failed to deserialize token response");

            // Calculate expiration time
            tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            return tokenResponse;
        }

        /// <summary>
        /// Refreshes the access token using refresh token
        /// </summary>
        public async Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                throw new ArgumentException("Refresh token cannot be null or empty", nameof(refreshToken));

            var requestBody = string.Format(
                "grant_type=refresh_token&refresh_token={0}&client_id={1}&client_secret={2}",
                Uri.EscapeDataString(refreshToken),
                Uri.EscapeDataString(_clientId),
                Uri.EscapeDataString(_clientSecret)
            );

            var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync(_tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Token refresh failed with status {response.StatusCode}: {errorContent}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);

            if (tokenResponse == null)
                throw new InvalidOperationException("Failed to deserialize token response");

            // Calculate expiration time
            tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            return tokenResponse;
        }

        /// <summary>
        /// Generates authorization URL for OAuth2 flow
        /// </summary>
        public string BuildAuthorizationUrl(string authEndpoint, string scopes, string state)
        {
            if (string.IsNullOrEmpty(authEndpoint))
                throw new ArgumentException("Auth endpoint cannot be null or empty", nameof(authEndpoint));
            if (string.IsNullOrEmpty(state))
                throw new ArgumentException("State cannot be null or empty", nameof(state));

            return string.Format(
                "{0}?client_id={1}&redirect_uri={2}&scope={3}&state={4}&access_type=offline&response_type=code",
                authEndpoint,
                Uri.EscapeDataString(_clientId),
                Uri.EscapeDataString(_redirectUri),
                Uri.EscapeDataString(scopes ?? string.Empty),
                Uri.EscapeDataString(state)
            );
        }

        /// <summary>
        /// Validates state parameter for CSRF protection
        /// </summary>
        public bool ValidateState(string providedState, string sessionState)
        {
            if (string.IsNullOrEmpty(providedState) || string.IsNullOrEmpty(sessionState))
                return false;

            return providedState == sessionState;
        }

        /// <summary>
        /// Checks if token is expired
        /// </summary>
        public bool IsTokenExpired(DateTime expiresAt)
        {
            return DateTime.UtcNow >= expiresAt;
        }
    }
}
