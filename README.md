# Fortnox Connect POC

A proof-of-concept ASP.NET 4.7 application demonstrating OAuth2 authentication flow with the Fortnox API. This application showcases a complete authentication implementation that allows other applications to interact with the Fortnox API through a secure proxy.

## Features

- **OAuth2 Authorization Code Flow**: Complete implementation of Fortnox's OAuth2 authentication
- **Secure Token Management**: Access and refresh tokens stored securely in session
- **Automatic Token Refresh**: Tokens are automatically refreshed before expiration
- **API Proxy**: Allows other applications to make authenticated requests to Fortnox
- **CSRF Protection**: State parameter validation to prevent CSRF attacks
- **Clean UI**: Simple, responsive interface for testing authentication flow

## Prerequisites

- Windows with IIS or IIS Express
- .NET Framework 4.7 or higher
- Visual Studio 2017 or higher (recommended)
- Fortnox Developer Account

## Setup Instructions

### 1. Register Your Application with Fortnox

1. Visit the [Fortnox Developer Portal](https://developer.fortnox.se/)
2. Create a new application
3. Configure the redirect URI: `https://localhost:44300/auth/callback`
4. Note your Client ID and Client Secret
5. Configure the required scopes (e.g., `companyinformation customer invoice`)

### 2. Configure the Application

1. Open `FortnoxConnect.sln` in Visual Studio
2. Edit `Web.config` and update the Fortnox configuration:

```xml
<add key="Fortnox:ClientId" value="YOUR_CLIENT_ID_HERE" />
<add key="Fortnox:ClientSecret" value="YOUR_CLIENT_SECRET_HERE" />
<add key="Fortnox:RedirectUri" value="https://localhost:44300/auth/callback" />
<add key="Fortnox:Scopes" value="companyinformation customer invoice" />
```

### 3. Build and Run

1. Restore NuGet packages:
   ```
   nuget restore FortnoxConnect.sln
   ```

2. Build the solution:
   ```
   msbuild FortnoxConnect.sln /p:Configuration=Release
   ```

3. Run the application:
   - Press F5 in Visual Studio, or
   - Deploy to IIS and navigate to the configured URL

### 4. Test the Authentication Flow

1. Navigate to `https://localhost:44300/`
2. Click "Connect to Fortnox"
3. Log in with your Fortnox credentials
4. Authorize the application
5. You'll be redirected back to the success page

## Architecture

### Authentication Flow

1. **Login Request** (`/auth/login`)
   - Generates state parameter for CSRF protection
   - Redirects to Fortnox authorization endpoint

2. **Callback Handler** (`/auth/callback`)
   - Validates state parameter
   - Exchanges authorization code for access token
   - Stores tokens in session

3. **Token Refresh** (`/auth/refreshtoken`)
   - Automatically refreshes expired tokens
   - Updates session with new tokens

### API Proxy

The `/api/proxy` endpoint allows other applications to make authenticated requests:

```
GET /api/proxy?endpoint=companyinformation
GET /api/proxy?endpoint=customers
```

Features:
- Automatic token refresh before making requests
- Proper error handling and status codes
- JSON response format

### Project Structure

```
FortnoxConnect/
├── Controllers/
│   ├── HomeController.cs      # Main UI pages
│   ├── AuthController.cs      # OAuth2 flow handlers
│   └── ApiController.cs       # API proxy endpoints
├── Models/
│   ├── FortnoxConfig.cs       # Configuration wrapper
│   └── TokenResponse.cs       # OAuth2 token model
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml       # Landing page
│   │   ├── Success.cshtml     # Post-authentication page
│   │   └── ApiTest.cshtml     # API testing page
│   └── Shared/
│       └── _Layout.cshtml     # Layout template
├── Content/
│   └── Site.css               # Stylesheet
└── Web.config                 # Configuration file
```

## Security Considerations

### Current Implementation (Development)

- Tokens stored in session memory
- State parameter for CSRF protection
- HTTPS required for production

### Production Recommendations

- Store tokens in encrypted database or secure vault (e.g., Azure Key Vault)
- Implement proper user authentication and authorization
- Use secure session management with appropriate timeouts
- Enable HSTS and other security headers
- Implement rate limiting on API endpoints
- Add logging and monitoring for security events
- Use environment variables for sensitive configuration

## Usage Examples

### For End Users

1. Navigate to the application homepage
2. Click "Connect to Fortnox"
3. Complete the authentication
4. Test API calls from the API Test page

### For Other Applications

Once authenticated, other applications can make requests through the proxy:

```http
GET https://localhost:44300/api/proxy?endpoint=companyinformation
Authorization: (handled by server session)
```

The proxy will:
- Validate the session has valid tokens
- Refresh tokens if expired
- Make the authenticated request to Fortnox
- Return the response to the caller

## Troubleshooting

### Common Issues

1. **"Invalid redirect URI"**
   - Ensure the redirect URI in Web.config matches exactly what's configured in Fortnox Developer Portal
   - Check that the protocol is HTTPS

2. **"Invalid client credentials"**
   - Verify Client ID and Client Secret are correct
   - Ensure there are no extra spaces or characters

3. **"Scope not granted"**
   - Check that the requested scopes are enabled in your Fortnox application settings

4. **SSL/Certificate errors**
   - For development, ensure IIS Express SSL certificate is trusted
   - For production, use a valid SSL certificate

## API Endpoints

### Authentication Endpoints

- `GET /auth/login` - Initiates OAuth2 flow
- `GET /auth/callback` - OAuth2 callback handler
- `GET /auth/refreshtoken` - Manually refresh token
- `GET /auth/logout` - Clear session

### Application Endpoints

- `GET /` - Home page
- `GET /home/success` - Post-authentication page
- `GET /home/apitest` - API testing interface

### API Proxy Endpoints

- `GET /api/proxy?endpoint={endpoint}` - Generic proxy endpoint
- `GET /api/getcompanyinfo` - Get company information

## Technologies Used

- ASP.NET MVC 5.2.7
- .NET Framework 4.7
- Newtonsoft.Json for JSON serialization
- System.Net.Http for HTTP requests
- Razor view engine

## License

This is a proof-of-concept application for demonstration purposes.

## Support

For issues related to:
- **This application**: Open an issue in this repository
- **Fortnox API**: Visit [Fortnox Developer Documentation](https://developer.fortnox.se/)
- **OAuth2**: See [OAuth 2.0 RFC](https://oauth.net/2/)

## Testing

This project includes a comprehensive unit test suite covering all core functionality.

### Test Coverage

The test suite includes **79 unit tests** covering:

- **TokenResponse Model** (11 tests): JSON serialization/deserialization, property validation
- **OAuth2 Authentication** (43 tests): Authorization code exchange, token refresh, CSRF protection
- **API Client** (25 tests): HTTP methods, authentication headers, error handling

### Running Tests

```bash
cd FortnoxConnect.Tests
dotnet test
```

For detailed test information, see [FortnoxConnect.Tests/README.md](FortnoxConnect.Tests/README.md).

### Test Results
- ✅ **Total Tests**: 79
- ✅ **Passed**: 79
- ⏱️ **Execution Time**: < 1 second

## Future Enhancements

Potential improvements for a production application:

- Database storage for tokens with encryption
- User management and multi-user support
- Webhook support for real-time updates
- More comprehensive error handling
- ~~Unit and integration tests~~ ✅ **Completed**
- Docker support
- CI/CD pipeline
- API rate limiting and throttling
- Request/response logging
- Health check endpoints