# Fortnox Connect - Quick Start Guide

Get up and running with Fortnox Connect in 5 minutes!

## Prerequisites Checklist

- [ ] Windows machine with .NET Framework 4.7+
- [ ] Visual Studio 2017 or later
- [ ] Fortnox developer account
- [ ] Fortnox company account for testing

## Step 1: Get Fortnox Credentials (2 minutes)

1. Go to [Fortnox Developer Portal](https://developer.fortnox.se/)
2. Create a new application
3. Set redirect URI: `https://localhost:44300/auth/callback`
4. Select scopes: `companyinformation`, `customer`, `invoice`
5. Copy your **Client ID** and **Client Secret**

## Step 2: Configure the Application (1 minute)

1. Open `FortnoxConnect/Web.config`
2. Find these lines and update them:

```xml
<add key="Fortnox:ClientId" value="PASTE_YOUR_CLIENT_ID_HERE" />
<add key="Fortnox:ClientSecret" value="PASTE_YOUR_CLIENT_SECRET_HERE" />
```

3. Save the file

## Step 3: Build and Run (2 minutes)

### Option A: Using Visual Studio (Easiest)

1. Open `FortnoxConnect.sln` in Visual Studio
2. Press **F5** to run
3. Your browser opens to `https://localhost:44300/`

### Option B: Using Command Line

```cmd
cd path\to\fortnox-connect-poc
nuget restore FortnoxConnect.sln
msbuild FortnoxConnect.sln /p:Configuration=Release
```

Then open in Visual Studio and press F5, or deploy to IIS.

## Step 4: Test Authentication

1. Click **"Connect to Fortnox"** button
2. Log in with your Fortnox credentials
3. Click **"Allow"** to grant permissions
4. You'll be redirected back to the success page

## Step 5: Test the API

1. Click **"Test API Calls"** on the success page
2. Click **"Get Company Information"**
3. See the JSON response with your company data

## That's It! 🎉

Your application is now authenticated and proxying requests to Fortnox!

## What's Next?

### For Development
- Read [SETUP.md](SETUP.md) for detailed configuration
- Review [ARCHITECTURE.md](ARCHITECTURE.md) to understand the code
- Check [API.md](API.md) for API documentation

### For Other Applications
Your external applications can now make requests like:

```http
GET https://localhost:44300/api/proxy?endpoint=customers
```

See [API.md](API.md) for full API reference.

## Common Issues

### Issue: "Invalid redirect URI"
**Fix**: Make sure Web.config and Fortnox Developer Portal both have exactly:
```
https://localhost:44300/auth/callback
```

### Issue: "Invalid client credentials"
**Fix**: Double-check your Client ID and Client Secret in Web.config

### Issue: SSL Certificate Warning
**Fix**: Click "Advanced" → "Proceed to localhost" in your browser

### Issue: Build Errors
**Fix**: 
1. Right-click solution → "Restore NuGet Packages"
2. Clean solution (Build → Clean Solution)
3. Rebuild (Build → Rebuild Solution)

## Need Help?

- 📖 Full setup guide: [SETUP.md](SETUP.md)
- 🏗️ Architecture details: [ARCHITECTURE.md](ARCHITECTURE.md)
- 🔌 API reference: [API.md](API.md)
- 📚 Fortnox docs: https://developer.fortnox.se/

## Security Reminder

⚠️ **Never commit your Client Secret to version control!**

For production:
- Use environment variables
- Store secrets in Azure Key Vault or similar
- Enable proper logging and monitoring

---

**Ready to build?** Open Visual Studio and press F5! 🚀
