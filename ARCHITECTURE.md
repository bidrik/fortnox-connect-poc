# Fortnox Connect - Architecture Documentation

This document describes the technical architecture, design decisions, and implementation details of the Fortnox Connect POC application.

## Overview

Fortnox Connect is an ASP.NET 4.7 MVC application that implements OAuth2 Authorization Code Flow to authenticate with the Fortnox API. It serves as both a demonstration of the authentication flow and a proxy service that allows other applications to make authenticated requests to Fortnox.

## Technology Stack

### Framework and Runtime
- **ASP.NET MVC 5.2.7**: Web application framework
- **.NET Framework 4.7**: Target runtime
- **C# 7.0**: Programming language

### Key Libraries
- **Newtonsoft.Json 12.0.2**: JSON serialization/deserialization
- **System.Net.Http**: HTTP client for making API requests
- **Microsoft.AspNet.Web.Optimization**: Bundling and minification
- **Razor View Engine**: Server-side templating

### Development Tools
- **Visual Studio 2017+**: IDE
- **MSBuild**: Build system
- **NuGet**: Package management
- **IIS Express**: Development web server

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                         Presentation Layer                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Views     │  │   Layout    │  │    Static Assets    │  │
│  │  (Razor)    │  │  Templates  │  │    (CSS, JS)        │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Controller Layer                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │    Home     │  │    Auth     │  │        API          │  │
│  │ Controller  │  │ Controller  │  │     Controller      │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                          Model Layer                          │
│  ┌─────────────┐  ┌─────────────┐                            │
│  │  Fortnox    │  │   Token     │                            │
│  │   Config    │  │  Response   │                            │
│  └─────────────┘  └─────────────┘                            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      External Services                        │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │               Fortnox OAuth2 & API                       │ │
│  │  • Authorization Endpoint                                 │ │
│  │  • Token Endpoint                                         │ │
│  │  • API Endpoints                                          │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## OAuth2 Authorization Code Flow

### Sequence Diagram

```
User          Browser         Application        Fortnox OAuth     Fortnox API
 |               |                  |                  |                |
 |---Click Login--->                |                  |                |
 |               |---GET /auth/login--->              |                |
 |               |                  |                  |                |
 |               |                  |--Generate State--|                |
 |               |                  |  (CSRF Token)    |                |
 |               |                  |                  |                |
 |               |<--Redirect to Fortnox Auth URL---  |                |
 |               |                  |                  |                |
 |               |--------GET /oauth-v1/auth--------->|                |
 |               |                  |                  |                |
 |<--Fortnox Login Page------------------------------ |                |
 |               |                  |                  |                |
 |---Enter Credentials------------->|                  |                |
 |               |                  |                  |                |
 |---Grant Access------------------>|                  |                |
 |               |                  |                  |                |
 |               |<--Redirect to Callback URL---------|                |
 |               |   (with code & state)              |                |
 |               |                  |                  |                |
 |               |---GET /auth/callback?code=XXX---->|                |
 |               |                  |                  |                |
 |               |                  |--Validate State--|                |
 |               |                  |                  |                |
 |               |                  |--POST /oauth-v1/token (code)---->|
 |               |                  |                  |                |
 |               |                  |<-Access Token & Refresh Token----|
 |               |                  |                  |                |
 |               |                  |--Store in Session|                |
 |               |                  |                  |                |
 |               |<--Redirect to Success Page---------                |
 |               |                  |                  |                |
 |---Make API Request-------------->|                  |                |
 |               |                  |                  |                |
 |               |                  |--Check Token Expiry             |
 |               |                  |                  |                |
 |               |                  |--GET /api/endpoint (Bearer Token)->
 |               |                  |                  |                |
 |               |                  |<----------API Response-----------|
 |               |                  |                  |                |
 |               |<------Response---|                  |                |
 |               |                  |                  |                |
```

### Flow Steps

1. **User Initiates Login** (`/auth/login`)
   - Generate random state parameter (GUID)
   - Store state in session
   - Build authorization URL with parameters:
     - `client_id`: Application identifier
     - `redirect_uri`: Callback URL
     - `scope`: Requested permissions
     - `state`: CSRF protection token
     - `access_type`: "offline" for refresh token
     - `response_type`: "code" for authorization code flow
   - Redirect user to Fortnox authorization endpoint

2. **User Authenticates with Fortnox**
   - User logs in with Fortnox credentials
   - Reviews and grants requested permissions
   - Fortnox redirects back with authorization code

3. **Application Receives Callback** (`/auth/callback`)
   - Validate state parameter matches session value
   - Extract authorization code from query string
   - Exchange code for tokens via POST to token endpoint

4. **Token Exchange**
   - POST to `https://apps.fortnox.se/oauth-v1/token` with:
     - `grant_type`: "authorization_code"
     - `code`: Authorization code from callback
     - `redirect_uri`: Must match initial request
     - `client_id`: Application identifier
     - `client_secret`: Application secret
   - Receive token response:
     - `access_token`: Used for API requests (expires in 1 hour)
     - `refresh_token`: Used to get new access tokens (valid for 45 days)
     - `expires_in`: Token lifetime in seconds
     - `token_type`: "Bearer"
     - `scope`: Granted permissions

5. **Token Storage**
   - Store tokens in session:
     - `AccessToken`: Current access token
     - `RefreshToken`: Refresh token
     - `TokenExpiresAt`: Calculated expiration time
   - Redirect to success page

## Controllers

### HomeController

**Purpose**: Handles main application pages and UI

**Actions**:
- `Index()`: Landing page with login button
- `Success()`: Post-authentication page showing success
- `ApiTest()`: Interactive API testing page

**Authorization**: None required for Index, session validation for Success and ApiTest

### AuthController

**Purpose**: Handles OAuth2 authentication flow

**Actions**:

1. **Login()**: `GET /auth/login`
   - Generates state token
   - Builds authorization URL
   - Redirects to Fortnox

2. **Callback()**: `GET /auth/callback`
   - Parameters: `code`, `state`, `error`
   - Validates state (CSRF protection)
   - Exchanges code for tokens
   - Stores tokens in session
   - Redirects to success page

3. **RefreshToken()**: `GET /auth/refreshtoken`
   - Retrieves refresh token from session
   - Exchanges refresh token for new access token
   - Updates session with new tokens
   - Redirects to success page

4. **Logout()**: `GET /auth/logout`
   - Clears session
   - Redirects to home page

**Helper Methods**:
- `ExchangeCodeForToken(string code)`: Exchanges authorization code for tokens
- `RefreshAccessToken(string refreshToken)`: Gets new access token using refresh token

### ApiController

**Purpose**: Proxy for Fortnox API requests, enabling other applications to make authenticated calls

**Actions**:

1. **Proxy()**: `GET /api/proxy?endpoint={endpoint}`
   - Parameters: `endpoint` - Fortnox API endpoint path
   - Retrieves access token from session
   - Checks token expiration
   - Automatically refreshes token if expired
   - Makes authenticated request to Fortnox
   - Returns response to caller

2. **GetCompanyInfo()**: `GET /api/getcompanyinfo`
   - Convenience method for common endpoint
   - Calls `Proxy("companyinformation")`

**Features**:
- Automatic token refresh
- Bearer token authentication
- JSON response handling
- Error handling with descriptive messages

## Models

### FortnoxConfig

**Purpose**: Centralized configuration management

**Properties**:
- `ClientId`: OAuth2 client identifier
- `ClientSecret`: OAuth2 client secret
- `RedirectUri`: OAuth2 callback URL
- `AuthEndpoint`: Fortnox authorization URL
- `TokenEndpoint`: Fortnox token exchange URL
- `ApiBaseUrl`: Fortnox API base URL
- `Scopes`: Requested API scopes (space-separated)

**Implementation**: Static properties reading from `ConfigurationManager.AppSettings`

### TokenResponse

**Purpose**: Represents OAuth2 token response from Fortnox

**Properties**:
- `AccessToken`: Bearer token for API requests
- `TokenType`: Token type (always "Bearer")
- `ExpiresIn`: Token lifetime in seconds
- `RefreshToken`: Token for refreshing access token
- `Scope`: Granted scopes
- `ExpiresAt`: Calculated expiration timestamp

**Serialization**: Uses Newtonsoft.Json attributes for JSON mapping

## Session Management

### Storage

- **Medium**: In-memory session state (ASP.NET Session)
- **Timeout**: 60 minutes (configurable in Web.config)
- **Mode**: InProc (single server)

### Stored Data

```
Session["OAuth_State"]      → string (GUID for CSRF protection)
Session["AccessToken"]      → string (Fortnox access token)
Session["RefreshToken"]     → string (Fortnox refresh token)
Session["TokenExpiresAt"]   → DateTime (token expiration time)
```

### Security Considerations

**Current Implementation (Development)**:
- Session stored in server memory
- Session cookie with HTTPS flag
- No persistent storage

**Production Recommendations**:
- Use distributed session state (Redis, SQL Server)
- Encrypt sensitive session data
- Implement secure session ID generation
- Set secure cookie attributes:
  - HttpOnly: true
  - Secure: true
  - SameSite: Strict

## Token Management

### Access Token Lifecycle

```
┌─────────────────────────────────────────────────────────────┐
│                      Token Lifecycle                          │
└─────────────────────────────────────────────────────────────┘

Initial Authentication
        │
        ▼
┌─────────────────┐
│  Get Access     │  Expires in 1 hour
│  Token          │  (3600 seconds)
└─────────────────┘
        │
        │ Use for API requests
        ▼
┌─────────────────┐
│  Token Valid?   │──No──┐
└─────────────────┘      │
        │ Yes             │
        │                 ▼
        │         ┌─────────────────┐
        │         │  Token Expired  │
        │         │  Refresh Needed │
        │         └─────────────────┘
        │                 │
        │                 │ Use Refresh Token
        │                 ▼
        │         ┌─────────────────┐
        │         │  Get New Access │
        │         │  Token          │
        │         └─────────────────┘
        │                 │
        │                 │ New token obtained
        │                 ▼
        │         ┌─────────────────┐
        │         │  Update Session │
        │         └─────────────────┘
        │                 │
        └─────────────────┘
                │
                ▼
        Make API Request
```

### Refresh Token Strategy

**Automatic Refresh**:
- Check token expiration before each API request
- If expired, automatically refresh using refresh token
- Update session with new tokens
- Proceed with original request

**Implementation**:
```csharp
// Check if token is expired
if (DateTime.UtcNow >= tokenExpiresAt)
{
    // Refresh token
    var newToken = await RefreshAccessToken(refreshToken);
    Session["AccessToken"] = newToken.AccessToken;
    Session["TokenExpiresAt"] = newToken.ExpiresAt;
}
```

### Token Refresh Flow

1. Detect token expiration
2. Retrieve refresh token from session
3. POST to token endpoint with:
   - `grant_type`: "refresh_token"
   - `refresh_token`: Current refresh token
   - `client_id`: Application ID
   - `client_secret`: Application secret
4. Receive new access token and refresh token
5. Update session with new values
6. Continue with original request

## Security Implementation

### CSRF Protection

**Mechanism**: OAuth2 state parameter

**Implementation**:
1. Generate random GUID on login
2. Store in session
3. Include in authorization URL
4. Validate on callback
5. Reject if mismatch

**Code**:
```csharp
// Generate and store
var state = Guid.NewGuid().ToString();
Session["OAuth_State"] = state;

// Validate
var sessionState = Session["OAuth_State"] as string;
if (state != sessionState) {
    return Error("CSRF attack detected");
}
```

### HTTPS Enforcement

- All endpoints require HTTPS
- SSL certificate required
- Redirect HTTP to HTTPS (configurable)

### Secure Token Storage

**Current** (Development):
- Server-side session storage
- No persistence between restarts
- Single-server only

**Recommended** (Production):
- Database with encryption at rest
- Azure Key Vault or similar for secrets
- Distributed cache for sessions
- Token encryption before storage

### Input Validation

- State parameter validation
- Authorization code format validation
- Error parameter checking
- Endpoint parameter sanitization

### Error Handling

- Generic error messages to users
- Detailed logging server-side
- No sensitive data in error responses
- Proper HTTP status codes

## API Integration

### Request Flow

```
Application → ApiController → Fortnox API
                ↓
         Token Management
                ↓
         Session Validation
                ↓
         Authorization Header
                ↓
         HTTP Request
```

### Request Headers

```http
GET /3/companyinformation HTTP/1.1
Host: api.fortnox.se
Authorization: Bearer {access_token}
Accept: application/json
```

### Response Handling

- Success: Return JSON response to caller
- 401 Unauthorized: Refresh token and retry
- Other errors: Return error JSON with details

### Supported Endpoints

The proxy supports any Fortnox API v3 endpoint:
- `/companyinformation` - Company data
- `/customers` - Customer management
- `/invoices` - Invoice operations
- `/articles` - Product catalog
- And all other v3 endpoints

## Configuration

### Web.config Structure

```xml
<configuration>
  <appSettings>
    <!-- Fortnox OAuth2 Settings -->
    <add key="Fortnox:ClientId" value="..." />
    <add key="Fortnox:ClientSecret" value="..." />
    <add key="Fortnox:RedirectUri" value="..." />
    <add key="Fortnox:AuthEndpoint" value="..." />
    <add key="Fortnox:TokenEndpoint" value="..." />
    <add key="Fortnox:ApiBaseUrl" value="..." />
    <add key="Fortnox:Scopes" value="..." />
    
    <!-- ASP.NET MVC Settings -->
    <add key="webpages:Version" value="..." />
    ...
  </appSettings>
  
  <system.web>
    <compilation targetFramework="4.7" />
    <sessionState timeout="60" />
  </system.web>
</configuration>
```

### Environment-Specific Configuration

For different environments (Dev, Test, Production):

1. Use Web.config transformations
2. Create Web.Debug.config, Web.Release.config
3. Transform settings during build/publish

Example transform:
```xml
<configuration xmlns:xdt="...">
  <appSettings>
    <add key="Fortnox:ClientId" 
         value="production_client_id"
         xdt:Transform="SetAttributes" 
         xdt:Locator="Match(key)" />
  </appSettings>
</configuration>
```

## Scalability Considerations

### Current Limitations

- In-memory session (single server only)
- No caching layer
- Synchronous API calls
- No connection pooling configuration

### Scaling Recommendations

1. **Session Management**:
   - Use distributed cache (Redis, SQL Server)
   - Implement session affinity if using multiple servers
   - Consider stateless JWT-based authentication

2. **Caching**:
   - Cache frequently accessed API responses
   - Implement cache invalidation strategy
   - Use HTTP cache headers

3. **Async/Await**:
   - Already implemented for HTTP calls
   - Consider async all the way through

4. **Connection Pooling**:
   - Configure HttpClient reuse
   - Use IHttpClientFactory (.NET Core) pattern
   - Set appropriate timeout values

5. **Load Balancing**:
   - Use sticky sessions or distributed cache
   - Health check endpoints
   - Graceful degradation

## Error Handling Strategy

### Error Categories

1. **User Errors**:
   - Invalid credentials
   - Access denied
   - Show friendly messages

2. **Configuration Errors**:
   - Missing settings
   - Invalid redirect URI
   - Log and show setup instructions

3. **API Errors**:
   - Rate limiting
   - Service unavailable
   - Retry with exponential backoff

4. **System Errors**:
   - Unexpected exceptions
   - Log details
   - Show generic message to user

### Logging Strategy

**Current**: Basic exception handling

**Recommended**:
- Structured logging (Serilog, NLog)
- Log levels: Debug, Info, Warning, Error, Fatal
- Correlation IDs for request tracking
- Separate logs for security events
- PII redaction in logs

## Testing Strategy

### Unit Testing

**Testable Components**:
- Model classes
- Configuration helpers
- Token validation logic
- Error handling

**Mocking**:
- HttpClient for API calls
- Session for state management
- Configuration for settings

### Integration Testing

**Test Scenarios**:
- Full OAuth2 flow
- Token refresh mechanism
- API proxy functionality
- Error scenarios

### Manual Testing

**Test Cases**:
1. Initial authentication
2. Successful callback
3. API calls with valid token
4. Token expiration and refresh
5. Logout and re-authentication
6. CSRF attack simulation
7. Invalid credentials
8. Network failures

## Deployment

### IIS Deployment

1. Build in Release mode
2. Publish to file system
3. Create IIS application pool (.NET 4.7)
4. Create website in IIS
5. Configure SSL certificate
6. Set appropriate permissions
7. Configure Web.config
8. Test deployment

### Azure Deployment

Can be adapted to Azure App Service:
1. Convert to ASP.NET Core for better Azure support
2. Use Azure Key Vault for secrets
3. Use Azure Redis for session
4. Enable Application Insights
5. Configure custom domains and SSL

## Monitoring and Observability

### Recommended Metrics

- Authentication success/failure rate
- Token refresh frequency
- API response times
- Error rates by type
- Active sessions
- Request volume

### Health Checks

Implement endpoints:
- `/health` - Application health
- `/health/fortnox` - Fortnox API connectivity
- `/health/session` - Session store connectivity

## Future Enhancements

### Short Term

- [ ] Database storage for tokens
- [ ] User management system
- [ ] Request logging
- [ ] API rate limiting
- [ ] Swagger/OpenAPI documentation

### Long Term

- [ ] Migrate to ASP.NET Core
- [ ] Multi-tenancy support
- [ ] Webhook integration
- [ ] Real-time updates (SignalR)
- [ ] Admin dashboard
- [ ] Automated testing suite
- [ ] Docker containerization
- [ ] Kubernetes deployment

## References

- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [Fortnox API Documentation](https://developer.fortnox.se/)
- [ASP.NET MVC Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/)
- [OWASP Security Guidelines](https://owasp.org/)

---

**Document Version**: 1.0  
**Last Updated**: November 2025  
**Application Version**: 1.0.0
