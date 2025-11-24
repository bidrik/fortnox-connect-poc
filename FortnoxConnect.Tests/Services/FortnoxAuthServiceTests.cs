using System.Net;
using FortnoxConnect.Models;
using FortnoxConnect.Services;
using Moq;
using Moq.Protected;
using Xunit;

namespace FortnoxConnect.Tests.Services
{
    /// <summary>
    /// Unit tests for FortnoxAuthService
    /// </summary>
    public class FortnoxAuthServiceTests
    {
        private const string TestClientId = "test_client_id";
        private const string TestClientSecret = "test_client_secret";
        private const string TestRedirectUri = "https://localhost:44300/auth/callback";
        private const string TestTokenEndpoint = "https://api.fortnox.se/oauth/token";
        private const string TestAuthEndpoint = "https://api.fortnox.se/oauth/authorize";

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FortnoxAuthService(
                null!, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint));
        }

        [Fact]
        public void Constructor_WithNullClientId_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FortnoxAuthService(
                httpClient, null!, TestClientSecret, TestRedirectUri, TestTokenEndpoint));
        }

        [Fact]
        public void Constructor_WithNullClientSecret_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FortnoxAuthService(
                httpClient, TestClientId, null!, TestRedirectUri, TestTokenEndpoint));
        }

        [Fact]
        public void Constructor_WithNullRedirectUri_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, null!, TestTokenEndpoint));
        }

        [Fact]
        public void Constructor_WithNullTokenEndpoint_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task ExchangeCodeForTokenAsync_WithNullCode_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.ExchangeCodeForTokenAsync(null!));
        }

        [Fact]
        public async Task ExchangeCodeForTokenAsync_WithEmptyCode_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.ExchangeCodeForTokenAsync(string.Empty));
        }

        [Fact]
        public async Task ExchangeCodeForTokenAsync_WithValidCode_ShouldReturnTokenResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var tokenResponseJson = @"{
                ""access_token"": ""test_access_token"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600,
                ""refresh_token"": ""test_refresh_token"",
                ""scope"": ""companyinformation""
            }";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponseJson)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act
            var result = await service.ExchangeCodeForTokenAsync("test_auth_code");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test_access_token", result.AccessToken);
            Assert.Equal("Bearer", result.TokenType);
            Assert.Equal(3600, result.ExpiresIn);
            Assert.Equal("test_refresh_token", result.RefreshToken);
            Assert.Equal("companyinformation", result.Scope);
            Assert.True(result.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task ExchangeCodeForTokenAsync_WithHttpError_ShouldThrowHttpRequestException()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid authorization code")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.ExchangeCodeForTokenAsync("invalid_code"));
        }

        [Fact]
        public async Task ExchangeCodeForTokenAsync_ShouldSendCorrectRequestBody()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var tokenResponseJson = @"{
                ""access_token"": ""token"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600,
                ""refresh_token"": ""refresh"",
                ""scope"": ""scope""
            }";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponseJson)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act
            await service.ExchangeCodeForTokenAsync("test_code");

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest.Method);
            var requestBody = await capturedRequest.Content!.ReadAsStringAsync();
            Assert.Contains("grant_type=authorization_code", requestBody);
            Assert.Contains($"code={Uri.EscapeDataString("test_code")}", requestBody);
            Assert.Contains($"client_id={Uri.EscapeDataString(TestClientId)}", requestBody);
            Assert.Contains($"client_secret={Uri.EscapeDataString(TestClientSecret)}", requestBody);
            Assert.Contains($"redirect_uri={Uri.EscapeDataString(TestRedirectUri)}", requestBody);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithNullRefreshToken_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.RefreshAccessTokenAsync(null!));
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithEmptyRefreshToken_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.RefreshAccessTokenAsync(string.Empty));
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithValidToken_ShouldReturnNewTokenResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var tokenResponseJson = @"{
                ""access_token"": ""new_access_token"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600,
                ""refresh_token"": ""new_refresh_token"",
                ""scope"": ""companyinformation customer""
            }";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponseJson)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act
            var result = await service.RefreshAccessTokenAsync("old_refresh_token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new_access_token", result.AccessToken);
            Assert.Equal("new_refresh_token", result.RefreshToken);
            Assert.True(result.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithHttpError_ShouldThrowHttpRequestException()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Invalid refresh token")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                service.RefreshAccessTokenAsync("invalid_refresh_token"));
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ShouldSendCorrectRequestBody()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var tokenResponseJson = @"{
                ""access_token"": ""token"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600,
                ""refresh_token"": ""refresh"",
                ""scope"": ""scope""
            }";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponseJson)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act
            await service.RefreshAccessTokenAsync("my_refresh_token");

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest.Method);
            var requestBody = await capturedRequest.Content!.ReadAsStringAsync();
            Assert.Contains("grant_type=refresh_token", requestBody);
            Assert.Contains($"refresh_token={Uri.EscapeDataString("my_refresh_token")}", requestBody);
            Assert.Contains($"client_id={Uri.EscapeDataString(TestClientId)}", requestBody);
            Assert.Contains($"client_secret={Uri.EscapeDataString(TestClientSecret)}", requestBody);
        }

        [Fact]
        public void BuildAuthorizationUrl_WithNullAuthEndpoint_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.BuildAuthorizationUrl(null!, "scopes", "state"));
        }

        [Fact]
        public void BuildAuthorizationUrl_WithNullState_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.BuildAuthorizationUrl(TestAuthEndpoint, "scopes", null!));
        }

        [Fact]
        public void BuildAuthorizationUrl_WithValidParameters_ShouldReturnCorrectUrl()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);
            var state = "test_state_123";
            var scopes = "companyinformation customer invoice";

            // Act
            var result = service.BuildAuthorizationUrl(TestAuthEndpoint, scopes, state);

            // Assert
            Assert.Contains(TestAuthEndpoint, result);
            Assert.Contains($"client_id={Uri.EscapeDataString(TestClientId)}", result);
            Assert.Contains($"redirect_uri={Uri.EscapeDataString(TestRedirectUri)}", result);
            Assert.Contains($"scope={Uri.EscapeDataString(scopes)}", result);
            Assert.Contains($"state={Uri.EscapeDataString(state)}", result);
            Assert.Contains("access_type=offline", result);
            Assert.Contains("response_type=code", result);
        }

        [Fact]
        public void BuildAuthorizationUrl_WithSpecialCharactersInScopes_ShouldEscapeCorrectly()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);
            var state = "state";
            var scopes = "scope1 scope2 scope3";

            // Act
            var result = service.BuildAuthorizationUrl(TestAuthEndpoint, scopes, state);

            // Assert
            Assert.Contains(Uri.EscapeDataString(scopes), result);
        }

        [Fact]
        public void ValidateState_WithMatchingStates_ShouldReturnTrue()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);
            var state = "matching_state_123";

            // Act
            var result = service.ValidateState(state, state);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateState_WithDifferentStates_ShouldReturnFalse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act
            var result = service.ValidateState("state1", "state2");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateState_WithNullProvidedState_ShouldReturnFalse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act
            var result = service.ValidateState(null!, "session_state");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateState_WithNullSessionState_ShouldReturnFalse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act
            var result = service.ValidateState("provided_state", null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateState_WithEmptyStates_ShouldReturnFalse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);

            // Act
            var result = service.ValidateState(string.Empty, string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsTokenExpired_WithExpiredToken_ShouldReturnTrue()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);
            var expiredTime = DateTime.UtcNow.AddHours(-1);

            // Act
            var result = service.IsTokenExpired(expiredTime);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTokenExpired_WithValidToken_ShouldReturnFalse()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);
            var futureTime = DateTime.UtcNow.AddHours(1);

            // Act
            var result = service.IsTokenExpired(futureTime);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsTokenExpired_WithCurrentTime_ShouldReturnTrue()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);
            var currentTime = DateTime.UtcNow;

            // Act
            var result = service.IsTokenExpired(currentTime);

            // Assert
            // Token is considered expired if current time >= expiry time
            Assert.True(result);
        }

        [Theory]
        [InlineData(-3600)] // 1 hour ago
        [InlineData(-60)]   // 1 minute ago
        [InlineData(-1)]    // 1 second ago
        [InlineData(0)]     // now
        public void IsTokenExpired_WithPastOrCurrentTimes_ShouldReturnTrue(int secondsOffset)
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);
            var expiryTime = DateTime.UtcNow.AddSeconds(secondsOffset);

            // Act
            var result = service.IsTokenExpired(expiryTime);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(1)]     // 1 second from now
        [InlineData(60)]    // 1 minute from now
        [InlineData(3600)]  // 1 hour from now
        public void IsTokenExpired_WithFutureTimes_ShouldReturnFalse(int secondsOffset)
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new FortnoxAuthService(
                httpClient, TestClientId, TestClientSecret, TestRedirectUri, TestTokenEndpoint);
            var expiryTime = DateTime.UtcNow.AddSeconds(secondsOffset);

            // Act
            var result = service.IsTokenExpired(expiryTime);

            // Assert
            Assert.False(result);
        }
    }
}
