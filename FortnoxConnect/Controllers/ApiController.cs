using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using FortnoxConnect.Models;

namespace FortnoxConnect.Controllers
{
    /// <summary>
    /// API controller that proxies requests to Fortnox API
    /// This allows other applications to make API calls through this application
    /// </summary>
    public class ApiController : Controller
    {
        /// <summary>
        /// Proxies a GET request to Fortnox API
        /// Example: /api/proxy?endpoint=companyinformation
        /// </summary>
        public async Task<ActionResult> Proxy(string endpoint)
        {
            var accessToken = Session["AccessToken"] as string;
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return Json(new { error = "Not authenticated" }, JsonRequestBehavior.AllowGet);
            }

            // Check if token is expired
            var expiresAt = Session["TokenExpiresAt"] as DateTime?;
            if (expiresAt.HasValue && DateTime.UtcNow >= expiresAt.Value)
            {
                // Token is expired, refresh it
                var refreshToken = Session["RefreshToken"] as string;
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    try
                    {
                        var tokenResponse = await RefreshAccessToken(refreshToken);
                        Session["AccessToken"] = tokenResponse.AccessToken;
                        Session["RefreshToken"] = tokenResponse.RefreshToken;
                        Session["TokenExpiresAt"] = tokenResponse.ExpiresAt;
                        accessToken = tokenResponse.AccessToken;
                    }
                    catch (Exception ex)
                    {
                        return Json(new { error = $"Failed to refresh token: {ex.Message}" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var url = $"{FortnoxConfig.ApiBaseUrl}/{endpoint}";
                    var response = await client.GetAsync(url);
                    
                    var responseBody = await response.Content.ReadAsStringAsync();
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(new { 
                            error = $"API request failed with status {response.StatusCode}",
                            details = responseBody
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Content(responseBody, "application/json");
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = $"API request failed: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Gets company information from Fortnox
        /// Example endpoint demonstrating API usage
        /// </summary>
        public async Task<ActionResult> GetCompanyInfo()
        {
            return await Proxy("companyinformation");
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

                var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(FortnoxConfig.TokenEndpoint, content);
                
                response.EnsureSuccessStatusCode();
                
                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                
                // Calculate expiration time
                tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                
                return tokenResponse;
            }
        }
    }
}
