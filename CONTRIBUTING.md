# Contributing to Fortnox Connect POC

Thank you for your interest in contributing to Fortnox Connect! This document provides guidelines for contributing to the project.

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Create a new branch for your feature or bugfix
4. Make your changes
5. Test your changes thoroughly
6. Submit a pull request

## Development Setup

### Prerequisites
- Windows with .NET Framework 4.7 or higher
- Visual Studio 2017 or later
- Fortnox developer account for testing

### Setup Steps
1. Follow the [QUICKSTART.md](QUICKSTART.md) guide
2. Configure your development environment with test credentials
3. Ensure all tests pass before making changes

## Coding Standards

### C# Code Style
- Follow Microsoft's C# Coding Conventions
- Use meaningful variable and method names
- Add XML documentation comments for public methods and classes
- Keep methods focused and concise (under 50 lines when possible)

### Example:
```csharp
/// <summary>
/// Exchanges an authorization code for an access token
/// </summary>
/// <param name="code">The authorization code from OAuth2 callback</param>
/// <returns>Token response containing access and refresh tokens</returns>
private async Task<TokenResponse> ExchangeCodeForToken(string code)
{
    // Implementation
}
```

### Naming Conventions
- **Classes**: PascalCase (e.g., `TokenResponse`)
- **Methods**: PascalCase (e.g., `ExchangeCodeForToken`)
- **Properties**: PascalCase (e.g., `AccessToken`)
- **Private fields**: _camelCase with underscore (e.g., `_httpClient`)
- **Local variables**: camelCase (e.g., `accessToken`)

## Project Structure

```
FortnoxConnect/
├── Controllers/       # MVC controllers
├── Models/           # Data models
├── Views/            # Razor views
├── Content/          # CSS and static assets
├── App_Start/        # Application startup configuration
└── Properties/       # Assembly information
```

## Making Changes

### Adding a New Feature

1. Create a new branch from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Implement your feature following the coding standards

3. Update documentation:
   - Add/update XML comments in code
   - Update README.md if needed
   - Update API.md for new API endpoints
   - Update ARCHITECTURE.md for architectural changes

4. Test your changes thoroughly

5. Commit with a clear message:
   ```bash
   git commit -m "Add feature: description of your feature"
   ```

### Fixing a Bug

1. Create a new branch:
   ```bash
   git checkout -b bugfix/issue-description
   ```

2. Fix the bug with minimal changes

3. Add a test case if applicable

4. Commit with reference to issue:
   ```bash
   git commit -m "Fix #123: description of bug fix"
   ```

## Commit Message Guidelines

### Format
```
<type>: <subject>

<body>

<footer>
```

### Types
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code style changes (formatting, no logic change)
- **refactor**: Code refactoring
- **test**: Adding or updating tests
- **chore**: Maintenance tasks

### Examples
```
feat: Add automatic token refresh on API calls

Implement logic to check token expiration before each API request
and automatically refresh if needed using the refresh token.

Closes #45
```

```
fix: Resolve CSRF validation error in callback

The state parameter was being cleared before validation, causing
legitimate authentication attempts to fail.

Fixes #67
```

## Pull Request Process

1. **Update Documentation**: Ensure all relevant documentation is updated

2. **Test Thoroughly**: 
   - Test the OAuth2 flow end-to-end
   - Test API proxy functionality
   - Test error scenarios

3. **Create Pull Request**:
   - Use a clear, descriptive title
   - Describe what changes you made and why
   - Reference any related issues
   - Include screenshots for UI changes

4. **Pull Request Template**:
   ```markdown
   ## Description
   Brief description of changes
   
   ## Type of Change
   - [ ] Bug fix
   - [ ] New feature
   - [ ] Documentation update
   - [ ] Code refactoring
   
   ## Testing
   Describe how you tested your changes
   
   ## Checklist
   - [ ] Code follows project style guidelines
   - [ ] Documentation updated
   - [ ] All tests pass
   - [ ] No new warnings introduced
   
   ## Related Issues
   Closes #(issue number)
   ```

5. **Code Review**: Address any feedback from reviewers

6. **Merge**: Once approved, your PR will be merged

## Security Issues

If you discover a security vulnerability:

1. **DO NOT** open a public issue
2. Email the details to the project maintainers
3. Include:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

## Testing Guidelines

### Manual Testing Checklist
- [ ] OAuth2 flow completes successfully
- [ ] Tokens are stored correctly in session
- [ ] Token refresh works automatically
- [ ] API proxy returns correct responses
- [ ] Error handling works as expected
- [ ] CSRF protection validates correctly
- [ ] Logout clears session properly

### Test Scenarios
1. **Happy Path**: Complete authentication and make API calls
2. **Token Expiration**: Verify automatic refresh
3. **Invalid Credentials**: Test error handling
4. **CSRF Attack**: Verify state validation
5. **Session Timeout**: Test re-authentication
6. **Network Errors**: Test error handling

## Documentation Updates

When making changes, update relevant documentation:

- **README.md**: Overview and general information
- **QUICKSTART.md**: Quick start guide
- **SETUP.md**: Detailed setup instructions
- **ARCHITECTURE.md**: Technical architecture
- **API.md**: API reference
- **Code Comments**: Inline documentation

## Questions or Need Help?

- Open an issue for questions about the codebase
- Tag maintainers for urgent matters
- Check existing issues and PRs for similar discussions

## Code of Conduct

### Our Standards
- Be respectful and inclusive
- Welcome newcomers
- Focus on constructive feedback
- Collaborate openly

### Unacceptable Behavior
- Harassment or discriminatory language
- Personal attacks
- Publishing others' private information
- Trolling or inflammatory comments

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Recognition

Contributors will be recognized in the project:
- Listed in CONTRIBUTORS.md (to be created)
- Mentioned in release notes for significant contributions

## Thank You!

Your contributions make this project better for everyone. We appreciate your time and effort! 🙏

---

**Questions?** Open an issue or contact the maintainers.
