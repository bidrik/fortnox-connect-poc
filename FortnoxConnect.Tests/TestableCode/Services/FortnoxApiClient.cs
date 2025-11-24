using System.Net;
using System.Net.Http.Headers;
using FortnoxConnect.Models;
using Newtonsoft.Json;

namespace FortnoxConnect.Services
{
    /// <summary>
    /// Service for making authenticated requests to Fortnox API
    /// This is a testable version extracted from ApiController
    /// </summary>
    public class FortnoxApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public FortnoxApiClient(HttpClient httpClient, string apiBaseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiBaseUrl = apiBaseUrl ?? throw new ArgumentNullException(nameof(apiBaseUrl));
        }

        /// <summary>
        /// Makes a GET request to Fortnox API
        /// </summary>
        public async Task<string> GetAsync(string endpoint, string accessToken)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token cannot be null or empty", nameof(accessToken));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = $"{_apiBaseUrl}/{endpoint}";
            var response = await _httpClient.GetAsync(url);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"API request failed with status {response.StatusCode}: {responseBody}");
            }

            return responseBody;
        }

        /// <summary>
        /// Makes a POST request to Fortnox API
        /// </summary>
        public async Task<string> PostAsync(string endpoint, string accessToken, string jsonBody)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token cannot be null or empty", nameof(accessToken));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = $"{_apiBaseUrl}/{endpoint}";
            var content = new StringContent(jsonBody ?? string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"API request failed with status {response.StatusCode}: {responseBody}");
            }

            return responseBody;
        }

        /// <summary>
        /// Makes a PUT request to Fortnox API
        /// </summary>
        public async Task<string> PutAsync(string endpoint, string accessToken, string jsonBody)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token cannot be null or empty", nameof(accessToken));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = $"{_apiBaseUrl}/{endpoint}";
            var content = new StringContent(jsonBody ?? string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"API request failed with status {response.StatusCode}: {responseBody}");
            }

            return responseBody;
        }

        /// <summary>
        /// Makes a DELETE request to Fortnox API
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint, string accessToken)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token cannot be null or empty", nameof(accessToken));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = $"{_apiBaseUrl}/{endpoint}";
            var response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"API request failed with status {response.StatusCode}: {responseBody}");
            }

            return true;
        }

        /// <summary>
        /// Gets company information from Fortnox
        /// </summary>
        public async Task<string> GetCompanyInformationAsync(string accessToken)
        {
            return await GetAsync("companyinformation", accessToken);
        }

        /// <summary>
        /// Gets customers from Fortnox
        /// </summary>
        public async Task<string> GetCustomersAsync(string accessToken)
        {
            return await GetAsync("customers", accessToken);
        }

        /// <summary>
        /// Gets invoices from Fortnox
        /// </summary>
        public async Task<string> GetInvoicesAsync(string accessToken)
        {
            return await GetAsync("invoices", accessToken);
        }
    }
}
