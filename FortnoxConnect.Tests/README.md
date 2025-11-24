# FortnoxConnect.Tests

Comprehensive unit test suite for the Fortnox API client implementation.

## Overview

This test project provides complete test coverage for the Fortnox Connect POC application, focusing on:
- OAuth2 authentication flow
- Token management
- API client operations
- Error handling and validation

## Test Structure

```
FortnoxConnect.Tests/
├── Models/
│   └── TokenResponseTests.cs          # Tests for OAuth2 token response model
├── Services/
│   ├── FortnoxAuthServiceTests.cs     # Tests for authentication service
│   └── FortnoxApiClientTests.cs       # Tests for API client
└── TestableCode/
    ├── Models/
    │   └── TokenResponse.cs           # Testable version of token model
    └── Services/
        ├── FortnoxAuthService.cs      # Testable authentication service
        └── FortnoxApiClient.cs        # Testable API client
```

## Test Coverage

### TokenResponse Model Tests (11 tests)
- Property setters and getters
- JSON serialization/deserialization with Newtonsoft.Json
- Snake_case property naming for API compatibility
- Handling of null values and empty JSON
- Token expiration calculation
- Various expires_in values (0, 60, 3600, 7200, 86400 seconds)

### FortnoxAuthService Tests (43 tests)
- **Constructor validation**: Ensures all required parameters are provided
- **ExchangeCodeForToken**: 
  - Validates authorization code exchange
  - Tests request body format
  - Handles HTTP errors (BadRequest, Unauthorized)
  - Verifies token expiration calculation
- **RefreshAccessToken**:
  - Tests token refresh flow
  - Validates request body format
  - Handles expired/invalid tokens
- **BuildAuthorizationUrl**:
  - Generates correct OAuth2 authorization URLs
  - Properly escapes special characters
  - Includes all required parameters
- **ValidateState**:
  - CSRF protection state validation
  - Handles null and empty values
- **IsTokenExpired**:
  - Tests token expiration logic
  - Handles past, current, and future times

### FortnoxApiClient Tests (25 tests)
- **Constructor validation**: Ensures HttpClient and API base URL are provided
- **GET requests**:
  - Sets proper authorization headers (Bearer token)
  - Sets Accept header for JSON responses
  - Constructs correct API URLs
  - Handles various HTTP error codes (400, 401, 403, 404, 500)
- **POST requests**:
  - Sends JSON content correctly
  - Validates required parameters
  - Uses POST method
- **PUT requests**:
  - Updates resources correctly
  - Uses PUT method
- **DELETE requests**:
  - Deletes resources successfully
  - Uses DELETE method
  - Returns boolean success indicator
- **Convenience methods**:
  - GetCompanyInformationAsync
  - GetCustomersAsync
  - GetInvoicesAsync

## Running the Tests

### Prerequisites
- .NET 10.0 SDK or later
- All dependencies are automatically restored via NuGet

### Run All Tests
```bash
cd FortnoxConnect.Tests
dotnet test
```

### Run Tests with Detailed Output
```bash
dotnet test --verbosity normal
```

### Run Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~FortnoxAuthServiceTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "FullyQualifiedName~ExchangeCodeForTokenAsync_WithValidCode_ShouldReturnTokenResponse"
```

## Test Patterns and Best Practices

### Naming Convention
Tests follow the pattern: `MethodName_StateUnderTest_ExpectedBehavior`

Examples:
- `ExchangeCodeForTokenAsync_WithValidCode_ShouldReturnTokenResponse`
- `ValidateState_WithMatchingStates_ShouldReturnTrue`
- `IsTokenExpired_WithExpiredToken_ShouldReturnTrue`

### Arrange-Act-Assert (AAA) Pattern
All tests follow the AAA pattern:
```csharp
[Fact]
public void TestName()
{
    // Arrange - Set up test data and mocks
    var service = new Service();
    
    // Act - Execute the method under test
    var result = service.Method();
    
    // Assert - Verify the expected outcome
    Assert.Equal(expected, result);
}
```

### Mocking with Moq
HTTP requests are mocked using Moq to avoid external dependencies:
```csharp
var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
mockHttpMessageHandler
    .Protected()
    .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
    .ReturnsAsync(new HttpResponseMessage { ... });
```

### Theory Tests for Multiple Scenarios
Theory tests validate behavior across multiple inputs:
```csharp
[Theory]
[InlineData(0)]
[InlineData(60)]
[InlineData(3600)]
public void Method_WithDifferentValues_ShouldWork(int value)
{
    // Test implementation
}
```

## Dependencies

- **xUnit 2.9.3**: Testing framework
- **Moq 4.20.72**: Mocking framework for HttpClient and dependencies
- **Newtonsoft.Json 13.0.4**: JSON serialization (matching main project)
- **Microsoft.NET.Test.Sdk 17.14.1**: Test execution
- **coverlet.collector 6.0.4**: Code coverage collection

## Key Testing Features

### 1. Isolation
Each test is independent and doesn't rely on external services or databases.

### 2. Comprehensive Error Testing
Tests cover both success and failure scenarios, including:
- Null/empty parameter validation
- HTTP error responses (400, 401, 403, 404, 500)
- Invalid tokens and expired credentials

### 3. Real-World Scenarios
Tests simulate actual Fortnox API responses and OAuth2 flows.

### 4. Request Validation
Tests verify that outgoing HTTP requests have correct:
- Headers (Authorization, Accept)
- Methods (GET, POST, PUT, DELETE)
- URLs and endpoints
- Request bodies

## Integration with CI/CD

These tests can be integrated into any CI/CD pipeline:

### GitHub Actions Example
```yaml
- name: Run tests
  run: dotnet test --no-build --verbosity normal
```

### Azure DevOps Example
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
```

## Test Results Summary

✅ **Total Tests**: 79  
✅ **Passed**: 79  
❌ **Failed**: 0  
⏱️ **Execution Time**: < 1 second

## Future Enhancements

Potential additions to the test suite:
1. Integration tests with test Fortnox API instance
2. Performance/load tests for API client
3. Tests for additional Fortnox API endpoints
4. Token refresh race condition tests
5. Concurrent request handling tests
6. WebSocket/real-time update tests (if applicable)

## Contributing

When adding new features to FortnoxConnect:
1. Add corresponding unit tests
2. Follow existing naming conventions
3. Maintain AAA pattern
4. Aim for high code coverage
5. Test both success and error cases
6. Mock external dependencies

## License

This test suite is part of the FortnoxConnect POC project.
