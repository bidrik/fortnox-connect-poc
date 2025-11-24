using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FortnoxConnect.Models;
using Newtonsoft.Json;

namespace FortnoxConnect.Controllers
{
    /// <summary>
    /// Controller handling OAuth2 authentication flow with Fortnox
    /// </summary>
    public class AuthController : Controller
    {
        /// <summary>
        /// Initiates OAuth2 authorization flow by redirecting to Fortnox
        /// </summary>
        public ActionResult Login()
        {
            // Generate random state for CSRF protection
            var state = Guid.NewGuid().ToString();
            Session["OAuth_State"] = state;

            // Build authorization URL
            var authUrl = string.Format(
                "{0}?client_id={1}&redirect_uri={2}&scope={3}&state={4}&access_type=offline&response_type=code",
                FortnoxConfig.AuthEndpoint,
                Uri.EscapeDataString(FortnoxConfig.ClientId),
                Uri.EscapeDataString(FortnoxConfig.RedirectUri),
                Uri.EscapeDataString(FortnoxConfig.Scopes),
                Uri.EscapeDataString(state)
            );

            return Redirect(authUrl);
        }

        /// <summary>
        /// Callback endpoint for OAuth2 redirect from Fortnox
        /// </summary>
        public async Task<ActionResult> Callback(string code, string state, string error)
        {
            // Check for errors
            if (!string.IsNullOrEmpty(error))
            {
                // Use Server.HtmlEncode to prevent XSS
                ViewBag.Error = $"Authorization error: {Server.HtmlEncode(error)}";
                return View("Error");
            }

            // Validate state to prevent CSRF attacks
            var sessionState = Session["OAuth_State"] as string;
            if (string.IsNullOrEmpty(state) || state != sessionState)
            {
                ViewBag.Error = "Invalid state parameter. Possible CSRF attack.";
                return View("Error");
            }

            // Clear the state from session
            Session.Remove("OAuth_State");

            // Exchange authorization code for access token
            try
            {
                var tokenResponse = await ExchangeCodeForToken(code);
                
                // Store tokens in session (in production, use secure storage)
                Session["AccessToken"] = tokenResponse.AccessToken;
                Session["RefreshToken"] = tokenResponse.RefreshToken;
                Session["TokenExpiresAt"] = tokenResponse.ExpiresAt;

                return RedirectToAction("Success", "Home");
            }
            catch (Exception ex)
            {
                // Use Server.HtmlEncode to prevent XSS from exception messages
                ViewBag.Error = $"Error exchanging code for token: {Server.HtmlEncode(ex.Message)}";
                return View("Error");
            }
        }

        /// <summary>
        /// Exchanges authorization code for access token
        /// </summary>
        private async Task<TokenResponse> ExchangeCodeForToken(string code)
        {
            using (var client = new HttpClient())
            {
                var requestBody = string.Format(
                    "grant_type=authorization_code&code={0}&redirect_uri={1}&client_id={2}&client_secret={3}",
                    Uri.EscapeDataString(code),
                    Uri.EscapeDataString(FortnoxConfig.RedirectUri),
                    Uri.EscapeDataString(FortnoxConfig.ClientId),
                    Uri.EscapeDataString(FortnoxConfig.ClientSecret)
                );

                var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(FortnoxConfig.TokenEndpoint, content);
                
                response.EnsureSuccessStatusCode();
                
                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                
                // Calculate expiration time
                tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                
                return tokenResponse;
            }
        }

        /// <summary>
        /// Refreshes an expired access token using refresh token
        /// </summary>
        public async Task<ActionResult> RefreshToken()
        {
            var refreshToken = Session["RefreshToken"] as string;
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var tokenResponse = await RefreshAccessToken(refreshToken);
                
                // Update tokens in session
                Session["AccessToken"] = tokenResponse.AccessToken;
                Session["RefreshToken"] = tokenResponse.RefreshToken;
                Session["TokenExpiresAt"] = tokenResponse.ExpiresAt;

                return RedirectToAction("Success", "Home");
            }
            catch (Exception ex)
            {
                // Use Server.HtmlEncode to prevent XSS from exception messages
                ViewBag.Error = $"Error refreshing token: {Server.HtmlEncode(ex.Message)}";
                return View("Error");
            }
        }

        /// <summary>
        /// Refreshes the access token using refresh token
        /// </summary>
        private async Task<TokenResponse> RefreshAccessToken(string refreshToken)
        {
            using (var client = new HttpClient())
            {
                var requestBody = string.Format(
                    "grant_type=refresh_token&refresh_token={0}&client_id={1}&client_secret={2}",
                    Uri.EscapeDataString(refreshToken),
                    Uri.EscapeDataString(FortnoxConfig.ClientId),
                    Uri.EscapeDataString(FortnoxConfig.ClientSecret)
                );

                var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(FortnoxConfig.TokenEndpoint, content);
                
                response.EnsureSuccessStatusCode();
                
                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                
                // Calculate expiration time
                tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                
                return tokenResponse;
            }
        }

        /// <summary>
        /// Logs out by clearing session
        /// </summary>
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
