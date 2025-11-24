# Troubleshooting Guide

This guide helps you resolve common issues when setting up and running Fortnox Connect.

## Table of Contents
- [Authentication Issues](#authentication-issues)
- [Configuration Issues](#configuration-issues)
- [Build and Deployment Issues](#build-and-deployment-issues)
- [Runtime Issues](#runtime-issues)
- [API Issues](#api-issues)
- [Network and SSL Issues](#network-and-ssl-issues)

---

## Authentication Issues

### Problem: "Invalid redirect URI" Error

**Symptoms**: After clicking "Connect to Fortnox", you see an error page with "Invalid redirect URI"

**Possible Causes**:
1. Mismatch between Web.config and Fortnox Developer Portal
2. HTTP vs HTTPS mismatch
3. Port number mismatch
4. Trailing slash difference

**Solutions**:

1. **Check Web.config**:
   ```xml
   <add key="Fortnox:RedirectUri" value="https://localhost:44300/auth/callback" />
   ```

2. **Check Fortnox Developer Portal**:
   - Go to your application settings
   - Verify redirect URI is exactly: `https://localhost:44300/auth/callback`
   - No trailing slash
   - HTTPS, not HTTP
   - Correct port number

3. **Common Mistakes**:
   - ❌ `http://localhost:44300/auth/callback` (HTTP instead of HTTPS)
   - ❌ `https://localhost:44300/auth/callback/` (trailing slash)
   - ❌ `https://localhost:5000/auth/callback` (wrong port)
   - ✅ `https://localhost:44300/auth/callback` (correct)

---

### Problem: "Invalid client credentials" Error

**Symptoms**: Error during token exchange after callback

**Possible Causes**:
1. Incorrect Client ID
2. Incorrect Client Secret
3. Expired credentials
4. Wrong Fortnox environment

**Solutions**:

1. **Verify Credentials in Web.config**:
   ```xml
   <add key="Fortnox:ClientId" value="YOUR_CLIENT_ID" />
   <add key="Fortnox:ClientSecret" value="YOUR_CLIENT_SECRET" />
   ```

2. **Check for Extra Characters**:
   - No spaces before or after the value
   - No quotes in the value
   - Copy-paste directly from Fortnox portal

3. **Regenerate Credentials**:
   - Go to Fortnox Developer Portal
   - Find your application
   - Regenerate Client Secret if needed
   - Update Web.config immediately

---

### Problem: "Invalid state parameter" / CSRF Error

**Symptoms**: "Invalid state parameter. Possible CSRF attack." message

**Possible Causes**:
1. Multiple authentication attempts
2. Session expired during authentication
3. Browser cookies disabled
4. Opening callback URL directly

**Solutions**:

1. **Clear Browser Data**:
   - Clear cookies for localhost
   - Clear session storage
   - Restart browser

2. **Check Session Configuration**:
   ```xml
   <sessionState mode="InProc" timeout="60" />
   ```

3. **Enable Cookies**:
   - Ensure browser allows cookies
   - Check for cookie-blocking extensions

4. **Retry Authentication**:
   - Go back to home page
   - Start authentication flow from beginning
   - Complete flow without interruption

---

## Configuration Issues

### Problem: "Missing Configuration" Error

**Symptoms**: Application crashes on startup with missing configuration

**Solutions**:

1. **Verify All Required Settings**:
   ```xml
   <add key="Fortnox:ClientId" value="..." />
   <add key="Fortnox:ClientSecret" value="..." />
   <add key="Fortnox:RedirectUri" value="..." />
   <add key="Fortnox:AuthEndpoint" value="https://apps.fortnox.se/oauth-v1/auth" />
   <add key="Fortnox:TokenEndpoint" value="https://apps.fortnox.se/oauth-v1/token" />
   <add key="Fortnox:ApiBaseUrl" value="https://api.fortnox.se/3" />
   <add key="Fortnox:Scopes" value="companyinformation customer invoice" />
   ```

2. **Check for Typos**:
   - Key names are case-sensitive
   - Use exact key names as shown above

3. **Verify Web.config Location**:
   - Must be in `FortnoxConnect` folder
   - Not in solution root

---

## Build and Deployment Issues

### Problem: Build Fails with Missing References

**Symptoms**: Build errors about missing assemblies or NuGet packages

**Solutions**:

1. **Restore NuGet Packages**:
   - **Visual Studio**: Right-click solution → "Restore NuGet Packages"
   - **Command Line**:
     ```cmd
     nuget restore FortnoxConnect.sln
     ```

2. **Clean and Rebuild**:
   - Build → Clean Solution
   - Build → Rebuild Solution

3. **Delete packages Folder**:
   ```cmd
   rmdir /s packages
   nuget restore FortnoxConnect.sln
   ```

4. **Check NuGet Sources**:
   - Tools → Options → NuGet Package Manager → Package Sources
   - Ensure nuget.org is enabled

---

### Problem: "Could not load file or assembly" Error

**Symptoms**: Runtime error about missing assembly

**Solutions**:

1. **Check Binding Redirects in Web.config**:
   ```xml
   <runtime>
     <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
       <dependentAssembly>
         <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" />
         <bindingRedirect oldVersion="0.0.0.0-12.0.0.0" newVersion="12.0.0.0" />
       </dependentAssembly>
     </assemblyBinding>
   </runtime>
   ```

2. **Rebuild Solution**:
   - Clean solution
   - Delete bin and obj folders
   - Rebuild

3. **Check Target Framework**:
   - Project Properties → Application
   - Verify Target Framework is .NET Framework 4.7

---

### Problem: Visual Studio Can't Find IIS Express

**Symptoms**: Can't run the application, IIS Express not found

**Solutions**:

1. **Repair Visual Studio Installation**:
   - Control Panel → Programs
   - Find Visual Studio
   - Click "Modify"
   - Select "Repair"

2. **Install IIS Express Separately**:
   - Download from Microsoft
   - Install manually

3. **Use Full IIS**:
   - Install IIS from Windows Features
   - Deploy to IIS instead of IIS Express

---

## Runtime Issues

### Problem: "Session Expired" or "Not Authenticated"

**Symptoms**: User is logged out unexpectedly, API calls fail

**Possible Causes**:
1. Session timeout (default 60 minutes)
2. Application pool recycle
3. Server restart
4. Session lost

**Solutions**:

1. **Increase Session Timeout**:
   ```xml
   <sessionState mode="InProc" timeout="120" />
   ```

2. **Use Persistent Session Storage** (Production):
   ```xml
   <sessionState mode="SQLServer" 
                 sqlConnectionString="..." 
                 timeout="120" />
   ```

3. **Re-authenticate**:
   - Click Logout
   - Start authentication flow again

---

### Problem: Token Refresh Fails

**Symptoms**: "Failed to refresh token" error

**Possible Causes**:
1. Refresh token expired (45 days)
2. Refresh token revoked
3. Network error
4. Invalid credentials

**Solutions**:

1. **Re-authenticate**:
   - Logout and login again
   - Fresh tokens will be issued

2. **Check Refresh Token Validity**:
   - Refresh tokens expire after 45 days
   - Must re-authenticate after expiration

3. **Check Network Connectivity**:
   - Verify connection to Fortnox API
   - Check firewall settings

---

## API Issues

### Problem: API Returns 401 Unauthorized

**Symptoms**: API calls fail with 401 status

**Possible Causes**:
1. Token expired
2. Invalid token
3. Not authenticated

**Solutions**:

1. **Check Authentication Status**:
   - Verify user has completed OAuth flow
   - Check session has valid tokens

2. **Manual Token Refresh**:
   - Navigate to `/auth/refreshtoken`

3. **Re-authenticate**:
   - Logout and login again

---

### Problem: API Returns 403 Forbidden

**Symptoms**: API calls fail with 403 status

**Possible Causes**:
1. Insufficient permissions/scopes
2. Resource not accessible
3. Account limitations

**Solutions**:

1. **Check Scopes in Web.config**:
   ```xml
   <add key="Fortnox:Scopes" value="companyinformation customer invoice" />
   ```

2. **Re-authenticate with Correct Scopes**:
   - Update scopes in Web.config
   - Logout and login again
   - Grant new permissions

3. **Verify Account Access**:
   - Ensure Fortnox account has access to resource
   - Check company permissions

---

### Problem: API Returns 429 Too Many Requests

**Symptoms**: Rate limit error from Fortnox

**Cause**: Exceeded Fortnox API rate limits (400 requests per 10 minutes)

**Solutions**:

1. **Implement Caching**:
   ```csharp
   // Cache responses to reduce API calls
   private static Dictionary<string, CachedData> _cache;
   ```

2. **Add Delays Between Requests**:
   ```csharp
   await Task.Delay(1000); // Wait 1 second
   ```

3. **Implement Exponential Backoff**:
   ```csharp
   int retryDelay = 1000;
   for (int i = 0; i < 3; i++)
   {
       try
       {
           return await MakeApiCall();
       }
       catch (RateLimitException)
       {
           await Task.Delay(retryDelay);
           retryDelay *= 2;
       }
   }
   ```

4. **Monitor Usage**:
   - Check Fortnox Developer Portal for usage stats
   - Implement request counting

---

## Network and SSL Issues

### Problem: SSL Certificate Warning in Browser

**Symptoms**: Browser shows "Your connection is not private" warning

**For Development (localhost)**:

1. **Trust IIS Express Certificate** (Windows):
   - Open IIS Express system tray icon
   - Right-click → Show All Applications
   - Find FortnoxConnect
   - Trust the certificate

2. **Proceed Anyway**:
   - Click "Advanced"
   - Click "Proceed to localhost (unsafe)"
   - This is safe for local development

**For Production**:

1. **Obtain Valid SSL Certificate**:
   - Purchase from Certificate Authority
   - Use Let's Encrypt (free)

2. **Install in IIS**:
   - Import certificate
   - Bind to website

---

### Problem: "Unable to connect to Fortnox" Error

**Symptoms**: Network errors when connecting to Fortnox

**Solutions**:

1. **Check Internet Connection**:
   - Verify internet access
   - Try accessing https://apps.fortnox.se/ in browser

2. **Check Firewall**:
   - Ensure outbound HTTPS is allowed
   - Whitelist Fortnox domains:
     - `*.fortnox.se`
     - `apps.fortnox.se`
     - `api.fortnox.se`

3. **Check Proxy Settings**:
   - If behind corporate proxy, configure Web.config:
   ```xml
   <system.net>
     <defaultProxy>
       <proxy proxyaddress="http://proxy.company.com:8080" />
     </defaultProxy>
   </system.net>
   ```

4. **DNS Issues**:
   - Try alternative DNS (8.8.8.8)
   - Flush DNS cache: `ipconfig /flushdns`

---

## Getting Additional Help

If you've tried the solutions above and still have issues:

1. **Check Logs**:
   - Review Windows Event Viewer
   - Check IIS logs
   - Look for exception details

2. **Enable Detailed Errors**:
   ```xml
   <system.web>
     <customErrors mode="Off"/>
   </system.web>
   ```

3. **Open an Issue**:
   - Visit the GitHub repository
   - Provide:
     - Error message (remove sensitive data)
     - Steps to reproduce
     - Your environment details
     - What you've tried

4. **Community Resources**:
   - [Fortnox Developer Documentation](https://developer.fortnox.se/)
   - [Fortnox Developer Forum](https://developer.fortnox.se/forum)
   - Stack Overflow (tag: fortnox-api)

---

## Diagnostic Checklist

Use this checklist to diagnose issues:

### Configuration
- [ ] Client ID and Secret are correct
- [ ] Redirect URI matches exactly
- [ ] All required app settings are present
- [ ] Scopes are configured correctly

### Network
- [ ] Internet connection is working
- [ ] Can access https://apps.fortnox.se/ in browser
- [ ] Firewall allows HTTPS traffic
- [ ] No proxy issues

### Application
- [ ] NuGet packages are restored
- [ ] Solution builds without errors
- [ ] Target framework is .NET 4.7
- [ ] IIS Express or IIS is configured correctly

### Authentication
- [ ] Can access home page
- [ ] Can initiate OAuth flow
- [ ] Can authenticate with Fortnox
- [ ] Gets redirected back successfully
- [ ] Tokens are stored in session

### API
- [ ] Can access authenticated pages
- [ ] API calls return data
- [ ] Token refresh works
- [ ] Error handling works

---

**Still Need Help?** Check the [SETUP.md](SETUP.md) for detailed configuration or open an issue on GitHub.
