# Fortnox Connect - API Documentation

This document describes the API endpoints provided by Fortnox Connect for external applications to interact with the Fortnox API through the authenticated proxy.

## Base URL

```
https://localhost:44300
```

**Note**: Replace with your actual deployment URL in production.

## Authentication

The Fortnox Connect application handles authentication with Fortnox using OAuth2. External applications can use the proxy endpoints without managing OAuth2 tokens themselves.

### Prerequisites

Before making API calls:
1. A user must have authenticated with Fortnox through the web interface
2. A valid session must exist with active tokens
3. The session must not have expired (default: 60 minutes)

## Endpoints

### 1. Authentication Endpoints

#### Initiate OAuth2 Flow

```http
GET /auth/login
```

**Description**: Redirects the user to Fortnox for authentication.

**Response**: HTTP 302 Redirect to Fortnox authorization page

**Example**:
```bash
curl -X GET "https://localhost:44300/auth/login"
```

---

#### OAuth2 Callback

```http
GET /auth/callback?code={code}&state={state}
```

**Description**: Handles the OAuth2 callback from Fortnox. This endpoint is called automatically by Fortnox after user authentication.

**Query Parameters**:
- `code` (string, required): Authorization code from Fortnox
- `state` (string, required): CSRF protection token
- `error` (string, optional): Error message if authentication failed

**Response**: HTTP 302 Redirect to success page or error page

---

#### Refresh Token

```http
GET /auth/refreshtoken
```

**Description**: Manually refreshes the access token using the refresh token.

**Response**: HTTP 302 Redirect to success page

**Note**: Token refresh is usually automatic when using the API proxy.

---

#### Logout

```http
GET /auth/logout
```

**Description**: Clears the session and logs out the user.

**Response**: HTTP 302 Redirect to home page

**Example**:
```bash
curl -X GET "https://localhost:44300/auth/logout"
```

---

### 2. API Proxy Endpoints

#### Generic Proxy Endpoint

```http
GET /api/proxy?endpoint={endpoint}
```

**Description**: Proxies a GET request to any Fortnox API v3 endpoint with automatic authentication and token management.

**Query Parameters**:
- `endpoint` (string, required): The Fortnox API endpoint path (without base URL)

**Request Headers**:
- None required (authentication handled by server session)

**Response**:
- **Success**: JSON response from Fortnox API
- **Error**: JSON error object

**Status Codes**:
- `200 OK`: Successful API call
- `401 Unauthorized`: Not authenticated
- `400 Bad Request`: Invalid endpoint parameter
- `500 Internal Server Error`: Server error

**Example Request**:
```bash
curl -X GET "https://localhost:44300/api/proxy?endpoint=companyinformation" \
     -H "Accept: application/json"
```

**Example Success Response**:
```json
{
  "CompanyInformation": {
    "CompanyName": "Example Company AB",
    "OrganizationNumber": "556677-8899",
    "VisitAddress": {
      "Address": "Example Street 123",
      "PostalCode": "12345",
      "City": "Stockholm",
      "Country": "SE"
    },
    "Email": "info@example.com",
    "Phone": "+46 8 123 456"
  }
}
```

**Example Error Response**:
```json
{
  "error": "Not authenticated"
}
```

---

#### Get Company Information

```http
GET /api/getcompanyinfo
```

**Description**: Convenience endpoint to retrieve company information. Equivalent to `/api/proxy?endpoint=companyinformation`.

**Request Headers**:
- None required

**Response**: JSON object with company information

**Example Request**:
```bash
curl -X GET "https://localhost:44300/api/getcompanyinfo" \
     -H "Accept: application/json"
```

**Example Response**:
```json
{
  "CompanyInformation": {
    "CompanyName": "Example Company AB",
    "OrganizationNumber": "556677-8899",
    "Address": "Example Street 123",
    "City": "Stockholm",
    "Country": "SE",
    "CountryCode": "SE",
    "Phone": "+46 8 123 456",
    "Email": "info@example.com"
  }
}
```

---

## Supported Fortnox API Endpoints

The proxy supports all Fortnox API v3 endpoints. Below are common examples:

### Company Information

```http
GET /api/proxy?endpoint=companyinformation
```

Get information about the company.

---

### Customers

```http
GET /api/proxy?endpoint=customers
```

List all customers.

**Query Parameters** (passed through):
- `page`: Page number
- `limit`: Results per page

**Example**:
```bash
curl "https://localhost:44300/api/proxy?endpoint=customers"
```

---

### Specific Customer

```http
GET /api/proxy?endpoint=customers/{customernumber}
```

Get details for a specific customer.

**Example**:
```bash
curl "https://localhost:44300/api/proxy?endpoint=customers/1"
```

---

### Invoices

```http
GET /api/proxy?endpoint=invoices
```

List all invoices.

**Example**:
```bash
curl "https://localhost:44300/api/proxy?endpoint=invoices"
```

---

### Articles

```http
GET /api/proxy?endpoint=articles
```

List all articles (products).

**Example**:
```bash
curl "https://localhost:44300/api/proxy?endpoint=articles"
```

---

### Orders

```http
GET /api/proxy?endpoint=orders
```

List all orders.

---

### Suppliers

```http
GET /api/proxy?endpoint=suppliers
```

List all suppliers.

---

## Error Handling

### Error Response Format

All errors are returned as JSON objects with an `error` field:

```json
{
  "error": "Error description",
  "details": "Optional detailed error information"
}
```

### Common Error Scenarios

#### 1. Not Authenticated

**Response**:
```json
{
  "error": "Not authenticated"
}
```

**Solution**: Ensure a user has completed the OAuth2 flow by visiting `/auth/login`.

---

#### 2. Token Expired (with failed refresh)

**Response**:
```json
{
  "error": "Failed to refresh token: {details}"
}
```

**Solution**: Logout and re-authenticate.

---

#### 3. Invalid Endpoint

**Response**:
```json
{
  "error": "API request failed with status NotFound",
  "details": "{error details from Fortnox}"
}
```

**Solution**: Check the endpoint parameter and Fortnox API documentation.

---

#### 4. Permission Denied

**Response**:
```json
{
  "error": "API request failed with status Forbidden",
  "details": "Insufficient permissions"
}
```

**Solution**: Ensure the required scopes are configured and granted during authentication.

---

## Request Examples

### Using cURL

#### Get Company Information
```bash
curl -X GET "https://localhost:44300/api/getcompanyinfo" \
     -H "Accept: application/json" \
     --insecure
```

#### Get Customers List
```bash
curl -X GET "https://localhost:44300/api/proxy?endpoint=customers" \
     -H "Accept: application/json" \
     --insecure
```

#### Get Specific Customer
```bash
curl -X GET "https://localhost:44300/api/proxy?endpoint=customers/1" \
     -H "Accept: application/json" \
     --insecure
```

---

### Using PowerShell

#### Get Company Information
```powershell
$response = Invoke-RestMethod -Uri "https://localhost:44300/api/getcompanyinfo" `
                               -Method Get `
                               -ContentType "application/json"
$response | ConvertTo-Json -Depth 10
```

#### Get Customers List
```powershell
$response = Invoke-RestMethod -Uri "https://localhost:44300/api/proxy?endpoint=customers" `
                               -Method Get `
                               -ContentType "application/json"
$response | ConvertTo-Json -Depth 10
```

---

### Using C#

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class FortnoxClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public FortnoxClient(string baseUrl = "https://localhost:44300")
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
        );
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var url = $"{_baseUrl}/api/proxy?endpoint={Uri.EscapeDataString(endpoint)}";
        var response = await _httpClient.GetAsync(url);
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
    }

    public async Task<string> GetCompanyInfoAsync()
    {
        var url = $"{_baseUrl}/api/getcompanyinfo";
        var response = await _httpClient.GetAsync(url);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }
}

// Usage
var client = new FortnoxClient();
var companyInfo = await client.GetCompanyInfoAsync();
Console.WriteLine(companyInfo);
```

---

### Using JavaScript/Node.js

```javascript
const axios = require('axios');

const baseUrl = 'https://localhost:44300';

async function getCompanyInfo() {
    try {
        const response = await axios.get(`${baseUrl}/api/getcompanyinfo`, {
            headers: {
                'Accept': 'application/json'
            }
        });
        console.log(response.data);
        return response.data;
    } catch (error) {
        console.error('Error:', error.response?.data || error.message);
    }
}

async function getFortnoxData(endpoint) {
    try {
        const response = await axios.get(`${baseUrl}/api/proxy`, {
            params: { endpoint },
            headers: {
                'Accept': 'application/json'
            }
        });
        return response.data;
    } catch (error) {
        console.error('Error:', error.response?.data || error.message);
    }
}

// Usage
getCompanyInfo();
getFortnoxData('customers');
```

---

### Using Python

```python
import requests
import json

BASE_URL = 'https://localhost:44300'

def get_company_info():
    """Get company information"""
    url = f'{BASE_URL}/api/getcompanyinfo'
    headers = {'Accept': 'application/json'}
    
    response = requests.get(url, headers=headers, verify=False)
    response.raise_for_status()
    
    return response.json()

def get_fortnox_data(endpoint):
    """Get data from Fortnox API via proxy"""
    url = f'{BASE_URL}/api/proxy'
    params = {'endpoint': endpoint}
    headers = {'Accept': 'application/json'}
    
    response = requests.get(url, params=params, headers=headers, verify=False)
    response.raise_for_status()
    
    return response.json()

# Usage
if __name__ == '__main__':
    try:
        company_info = get_company_info()
        print(json.dumps(company_info, indent=2))
        
        customers = get_fortnox_data('customers')
        print(json.dumps(customers, indent=2))
    except requests.exceptions.RequestException as e:
        print(f'Error: {e}')
```

---

## Rate Limiting

**Current Implementation**: No rate limiting

**Fortnox API Limits**:
- 400 requests per 10 minutes per access token
- Exceeded limits result in HTTP 429 Too Many Requests

**Recommendations**:
- Implement caching for frequently accessed data
- Use appropriate request intervals
- Handle 429 responses with exponential backoff
- Monitor usage through Fortnox Developer Portal

---

## Best Practices

### 1. Error Handling

Always handle errors gracefully:

```javascript
async function safeApiCall(endpoint) {
    try {
        const response = await fetch(`/api/proxy?endpoint=${endpoint}`);
        
        if (!response.ok) {
            const error = await response.json();
            console.error('API Error:', error);
            return null;
        }
        
        return await response.json();
    } catch (error) {
        console.error('Network Error:', error);
        return null;
    }
}
```

### 2. Caching

Cache responses to reduce API calls:

```csharp
// Simple in-memory cache
private static Dictionary<string, (DateTime, object)> _cache = new();
private static TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

public async Task<T> GetCachedAsync<T>(string endpoint)
{
    var cacheKey = $"fortnox_{endpoint}";
    
    if (_cache.TryGetValue(cacheKey, out var cached))
    {
        if (DateTime.UtcNow - cached.Item1 < _cacheExpiry)
        {
            return (T)cached.Item2;
        }
    }
    
    var data = await GetAsync<T>(endpoint);
    _cache[cacheKey] = (DateTime.UtcNow, data);
    
    return data;
}
```

### 3. Retry Logic

Implement retry with exponential backoff:

```python
import time
from requests.adapters import HTTPAdapter
from requests.packages.urllib3.util.retry import Retry

def get_session_with_retry():
    session = requests.Session()
    retry = Retry(
        total=3,
        backoff_factor=1,
        status_forcelist=[429, 500, 502, 503, 504]
    )
    adapter = HTTPAdapter(max_retries=retry)
    session.mount('https://', adapter)
    return session
```

### 4. Logging

Log all API interactions:

```csharp
public async Task<T> GetAsync<T>(string endpoint)
{
    var startTime = DateTime.UtcNow;
    
    try
    {
        var result = await _httpClient.GetAsync(...);
        
        _logger.LogInformation(
            "API Call: {Endpoint}, Duration: {Duration}ms, Status: {Status}",
            endpoint,
            (DateTime.UtcNow - startTime).TotalMilliseconds,
            result.StatusCode
        );
        
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "API Call Failed: {Endpoint}", endpoint);
        throw;
    }
}
```

---

## Security Considerations

### HTTPS Required

All API calls should use HTTPS in production:
```
https://your-domain.com/api/proxy?endpoint=...
```

### Session Management

- Sessions expire after 60 minutes of inactivity
- Re-authentication required after session expiration
- No API keys or tokens required from external applications

### CORS (Cross-Origin Resource Sharing)

If calling from a web application on a different domain, configure CORS in Web.config:

```xml
<system.webServer>
  <httpProtocol>
    <customHeaders>
      <add name="Access-Control-Allow-Origin" value="https://your-allowed-domain.com" />
      <add name="Access-Control-Allow-Methods" value="GET, POST, PUT, DELETE" />
      <add name="Access-Control-Allow-Headers" value="Content-Type" />
    </customHeaders>
  </httpProtocol>
</system.webServer>
```

---

## Support and Resources

- **Fortnox API Documentation**: https://developer.fortnox.se/
- **Fortnox API v3 Reference**: https://developer.fortnox.se/documentation/
- **OAuth 2.0 Specification**: https://oauth.net/2/

---

## Changelog

### Version 1.0.0 (November 2025)
- Initial release
- OAuth2 authentication flow
- Generic API proxy endpoint
- Company information endpoint
- Automatic token refresh
- Session-based authentication

---

**API Version**: 1.0  
**Last Updated**: November 2025  
**Fortnox API Version**: v3
