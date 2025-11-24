# Fortnox Connect - Detailed Setup Guide

This guide provides step-by-step instructions for setting up and running the Fortnox Connect POC application.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Fortnox Developer Account Setup](#fortnox-developer-account-setup)
3. [Application Configuration](#application-configuration)
4. [Building the Application](#building-the-application)
5. [Running the Application](#running-the-application)
6. [Testing the Authentication Flow](#testing-the-authentication-flow)
7. [Using the API Proxy](#using-the-api-proxy)
8. [Troubleshooting](#troubleshooting)

## Prerequisites

Before you begin, ensure you have:

- **Operating System**: Windows 10 or Windows Server 2016 or later
- **.NET Framework**: Version 4.7 or higher installed
- **Development Environment**: Visual Studio 2017 or later (Community, Professional, or Enterprise)
- **IIS**: IIS Express (included with Visual Studio) or full IIS
- **SSL Certificate**: For HTTPS (IIS Express includes a development certificate)
- **Fortnox Account**: A Fortnox company account with API access

### Checking Your Environment

1. Check .NET Framework version:
   - Open Command Prompt
   - Run: `reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release`
   - A value of 460798 or higher indicates .NET 4.7 or later

2. Check Visual Studio:
   - Open Visual Studio
   - Go to Help → About Microsoft Visual Studio
   - Ensure version is 2017 (15.x) or later

## Fortnox Developer Account Setup

### Step 1: Create a Developer Account

1. Navigate to [https://developer.fortnox.se/](https://developer.fortnox.se/)
2. Click "Sign Up" or "Create Account"
3. Follow the registration process
4. Verify your email address

### Step 2: Create an Application

1. Log in to the Fortnox Developer Portal
2. Navigate to "My Applications" or "Applications"
3. Click "Create New Application"
4. Fill in the application details:
   - **Name**: Fortnox Connect POC (or your preferred name)
   - **Description**: OAuth2 authentication proof-of-concept
   - **Application Type**: Web Application

### Step 3: Configure OAuth2 Settings

1. In your application settings, find the OAuth2 configuration section
2. Set the **Redirect URI**: `https://localhost:44300/auth/callback`
   - ⚠️ This must match exactly (including HTTPS and port)
3. Configure **Scopes**: Select the API scopes you need:
   - `companyinformation` - Company information access
   - `customer` - Customer data access
   - `invoice` - Invoice operations
   - Add others as needed for your use case

### Step 4: Get Your Credentials

1. After saving, you'll receive:
   - **Client ID**: A unique identifier for your application
   - **Client Secret**: A secret key (keep this secure!)
2. Copy both values - you'll need them for configuration

⚠️ **Security Note**: Never commit your Client Secret to version control or share it publicly!

## Application Configuration

### Step 1: Clone/Download the Repository

```bash
git clone https://github.com/bidrik/fortnox-connect-poc.git
cd fortnox-connect-poc
```

### Step 2: Open the Solution

1. Open `FortnoxConnect.sln` in Visual Studio
2. Wait for Visual Studio to restore NuGet packages automatically
3. If packages don't restore automatically:
   - Right-click the solution in Solution Explorer
   - Select "Restore NuGet Packages"

### Step 3: Configure Web.config

1. Open `FortnoxConnect/Web.config`
2. Locate the `<appSettings>` section
3. Update the following values:

```xml
<!-- Replace YOUR_CLIENT_ID_HERE with your actual Client ID -->
<add key="Fortnox:ClientId" value="YOUR_CLIENT_ID_HERE" />

<!-- Replace YOUR_CLIENT_SECRET_HERE with your actual Client Secret -->
<add key="Fortnox:ClientSecret" value="YOUR_CLIENT_SECRET_HERE" />

<!-- Verify this matches your Fortnox app configuration -->
<add key="Fortnox:RedirectUri" value="https://localhost:44300/auth/callback" />

<!-- Adjust scopes as needed (space-separated) -->
<add key="Fortnox:Scopes" value="companyinformation customer invoice" />
```

### Step 4: Verify SSL Configuration

1. Right-click the `FortnoxConnect` project in Solution Explorer
2. Select "Properties"
3. Go to the "Web" tab
4. Ensure:
   - "Enable SSL" is checked
   - "Project Url" is `https://localhost:44300/`
5. If prompted, click "Create Virtual Directory"

## Building the Application

### Using Visual Studio

1. Open the solution in Visual Studio
2. Select "Release" or "Debug" configuration from the toolbar
3. Click Build → Build Solution (or press Ctrl+Shift+B)
4. Wait for the build to complete
5. Check the Output window for any errors

### Using MSBuild (Command Line)

1. Open "Developer Command Prompt for VS" (from Start Menu)
2. Navigate to the solution directory:
   ```cmd
   cd path\to\fortnox-connect-poc
   ```
3. Restore NuGet packages:
   ```cmd
   nuget restore FortnoxConnect.sln
   ```
4. Build the solution:
   ```cmd
   msbuild FortnoxConnect.sln /p:Configuration=Release
   ```

### Verify Build Success

After building, check:
- `FortnoxConnect/bin/` directory contains compiled DLLs
- No build errors in the Output window
- All references are resolved

## Running the Application

### Option 1: Run from Visual Studio (Recommended for Development)

1. In Visual Studio, ensure `FortnoxConnect` is set as the startup project
2. Press F5 (Debug) or Ctrl+F5 (Run without debugging)
3. Your default browser will open to `https://localhost:44300/`
4. If prompted about the SSL certificate, click "Yes" to trust it

### Option 2: Deploy to IIS

#### IIS Setup

1. Open IIS Manager
2. Create a new Application Pool:
   - Right-click "Application Pools" → "Add Application Pool"
   - Name: `FortnoxConnectPool`
   - .NET CLR Version: v4.0
   - Managed Pipeline Mode: Integrated

3. Create a new Website:
   - Right-click "Sites" → "Add Website"
   - Site name: `FortnoxConnect`
   - Application pool: `FortnoxConnectPool`
   - Physical path: Point to `FortnoxConnect` folder
   - Binding:
     - Type: https
     - Port: 44300
     - SSL Certificate: Select your certificate

4. Ensure the application pool identity has read access to the application folder

5. Browse to `https://localhost:44300/`

## Testing the Authentication Flow

### Step 1: Initial Access

1. Navigate to `https://localhost:44300/`
2. You should see the home page with:
   - Application title
   - Feature list
   - "Connect to Fortnox" button
   - Setup instructions

### Step 2: Initiate Authentication

1. Click the "Connect to Fortnox" button
2. You'll be redirected to Fortnox's login page
3. The URL should be: `https://apps.fortnox.se/oauth-v1/auth?client_id=...`

### Step 3: Login to Fortnox

1. Enter your Fortnox credentials
2. If you have multiple companies, select the one to use
3. Click "Sign In"

### Step 4: Grant Authorization

1. Review the permissions requested by the application
2. Click "Allow" or "Authorize" to grant access
3. You'll be redirected back to your application

### Step 5: Verify Success

1. You should land on the Success page (`/home/success`)
2. The page should show:
   - Success message
   - Options to test the API or logout
3. Your session now contains valid access and refresh tokens

## Using the API Proxy

### Testing Through the UI

1. From the Success page, click "Test API Calls"
2. Click "Get Company Information" to test a simple API call
3. The response will be displayed in JSON format
4. Try custom endpoints:
   - Enter `customers` and click "Test Custom Endpoint"
   - Enter `invoices` to list invoices

### Testing Programmatically

Other applications can make requests to the proxy:

#### Get Company Information
```http
GET https://localhost:44300/api/getcompanyinfo
Accept: application/json
```

#### Custom Endpoint Proxy
```http
GET https://localhost:44300/api/proxy?endpoint=customers
Accept: application/json
```

#### Example with cURL
```bash
curl -X GET "https://localhost:44300/api/proxy?endpoint=companyinformation" \
     -H "accept: application/json" \
     --insecure
```

#### Example with PowerShell
```powershell
Invoke-RestMethod -Uri "https://localhost:44300/api/proxy?endpoint=companyinformation" `
                  -Method Get `
                  -ContentType "application/json"
```

### Understanding the Response

Successful response:
```json
{
  "CompanyInformation": {
    "CompanyName": "Example Company AB",
    "OrganizationNumber": "XXXXXX-XXXX",
    ...
  }
}
```

Error response:
```json
{
  "error": "Not authenticated"
}
```

## Troubleshooting

### Issue: "Invalid redirect_uri" Error

**Symptoms**: After clicking "Connect to Fortnox", you get an error about invalid redirect URI

**Solution**:
1. Check that Web.config has: `https://localhost:44300/auth/callback`
2. Verify Fortnox Developer Portal has the exact same URI
3. Ensure protocol is HTTPS (not HTTP)
4. Check the port number matches (44300)
5. No trailing slash in the URI

### Issue: "Invalid client credentials" Error

**Symptoms**: Error during token exchange

**Solution**:
1. Verify Client ID is correct (no spaces or extra characters)
2. Verify Client Secret is correct and not expired
3. Check that you're using the correct Fortnox account
4. Regenerate credentials in Fortnox Developer Portal if needed

### Issue: SSL Certificate Warning

**Symptoms**: Browser shows security warning about the certificate

**Solution for Development**:
1. In Visual Studio, IIS Express creates a self-signed certificate
2. Trust the certificate:
   - Click "Advanced" in the browser warning
   - Click "Proceed" or "Continue to localhost"
3. To permanently trust (Windows):
   - Open IIS Express system tray icon
   - Right-click and select "Show All Applications"
   - Find FortnoxConnect and trust the certificate

**Solution for Production**:
1. Obtain a valid SSL certificate from a Certificate Authority
2. Install the certificate in IIS
3. Bind the certificate to your site

### Issue: "Not authenticated" When Testing API

**Symptoms**: API proxy returns "Not authenticated" error

**Solution**:
1. Ensure you completed the authentication flow
2. Check that session is active (session timeout is 60 minutes by default)
3. Try logging out and authenticating again
4. Check browser console for JavaScript errors

### Issue: Build Fails with Missing References

**Symptoms**: Build errors about missing assemblies or NuGet packages

**Solution**:
1. Right-click solution → "Restore NuGet Packages"
2. Clean the solution: Build → Clean Solution
3. Rebuild: Build → Rebuild Solution
4. If still failing, delete `packages` folder and restore again

### Issue: Token Expired

**Symptoms**: API calls return 401 Unauthorized

**Solution**:
- The application should automatically refresh tokens
- If automatic refresh fails:
  1. Click "Logout"
  2. Authenticate again
  3. If persistent, check refresh token implementation

### Issue: Scope Not Granted

**Symptoms**: API calls fail with permission errors

**Solution**:
1. Check requested scopes in Web.config
2. Ensure scopes are enabled in Fortnox Developer Portal
3. Re-authenticate to get updated scopes
4. Some scopes may require approval from Fortnox

## Getting Help

If you encounter issues not covered here:

1. Check the [Fortnox Developer Documentation](https://developer.fortnox.se/)
2. Review the [OAuth 2.0 specification](https://oauth.net/2/)
3. Open an issue in this repository with:
   - Detailed description of the problem
   - Steps to reproduce
   - Error messages (with sensitive data removed)
   - Your environment details

## Next Steps

After successfully setting up:

1. Review the code to understand the OAuth2 flow
2. Explore the Fortnox API documentation for available endpoints
3. Implement additional features:
   - Database storage for tokens
   - User management
   - Additional API endpoints
   - Error logging and monitoring

## Security Checklist for Production

Before deploying to production:

- [ ] Use secure token storage (database with encryption or vault)
- [ ] Implement proper user authentication and authorization
- [ ] Use environment variables for secrets (not Web.config)
- [ ] Enable HTTPS with valid SSL certificate
- [ ] Implement rate limiting
- [ ] Add comprehensive error logging
- [ ] Set appropriate session timeouts
- [ ] Enable security headers (HSTS, CSP, etc.)
- [ ] Perform security audit
- [ ] Test error scenarios
- [ ] Document API usage for other applications

---

**Last Updated**: November 2025  
**Application Version**: 1.0.0  
**Target Framework**: .NET Framework 4.7
