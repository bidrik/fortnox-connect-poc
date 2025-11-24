using FortnoxConnect.Models;
using Newtonsoft.Json;
using Xunit;

namespace FortnoxConnect.Tests.Models
{
    /// <summary>
    /// Unit tests for TokenResponse model
    /// </summary>
    public class TokenResponseTests
    {
        [Fact]
        public void TokenResponse_Properties_ShouldBeSettable()
        {
            // Arrange & Act
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test_access_token",
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshToken = "test_refresh_token",
                Scope = "companyinformation customer invoice",
                ExpiresAt = DateTime.UtcNow.AddSeconds(3600)
            };

            // Assert
            Assert.Equal("test_access_token", tokenResponse.AccessToken);
            Assert.Equal("Bearer", tokenResponse.TokenType);
            Assert.Equal(3600, tokenResponse.ExpiresIn);
            Assert.Equal("test_refresh_token", tokenResponse.RefreshToken);
            Assert.Equal("companyinformation customer invoice", tokenResponse.Scope);
            Assert.NotEqual(default(DateTime), tokenResponse.ExpiresAt);
        }

        [Fact]
        public void TokenResponse_JsonDeserialization_ShouldMapCorrectly()
        {
            // Arrange
            var json = @"{
                ""access_token"": ""abc123"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 7200,
                ""refresh_token"": ""xyz789"",
                ""scope"": ""companyinformation""
            }";

            // Act
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

            // Assert
            Assert.NotNull(tokenResponse);
            Assert.Equal("abc123", tokenResponse.AccessToken);
            Assert.Equal("Bearer", tokenResponse.TokenType);
            Assert.Equal(7200, tokenResponse.ExpiresIn);
            Assert.Equal("xyz789", tokenResponse.RefreshToken);
            Assert.Equal("companyinformation", tokenResponse.Scope);
        }

        [Fact]
        public void TokenResponse_JsonSerialization_ShouldMapCorrectly()
        {
            // Arrange
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test_token",
                TokenType = "Bearer",
                ExpiresIn = 1800,
                RefreshToken = "test_refresh",
                Scope = "read write"
            };

            // Act
            var json = JsonConvert.SerializeObject(tokenResponse);

            // Assert
            Assert.Contains("\"access_token\":\"test_token\"", json);
            Assert.Contains("\"token_type\":\"Bearer\"", json);
            Assert.Contains("\"expires_in\":1800", json);
            Assert.Contains("\"refresh_token\":\"test_refresh\"", json);
            Assert.Contains("\"scope\":\"read write\"", json);
        }

        [Fact]
        public void TokenResponse_ExpiresAt_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            var beforeTime = DateTime.UtcNow;
            var expiresInSeconds = 3600;
            
            // Act
            var tokenResponse = new TokenResponse
            {
                ExpiresIn = expiresInSeconds,
                ExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds)
            };
            var afterTime = DateTime.UtcNow;

            // Assert
            Assert.True(tokenResponse.ExpiresAt >= beforeTime.AddSeconds(expiresInSeconds));
            Assert.True(tokenResponse.ExpiresAt <= afterTime.AddSeconds(expiresInSeconds));
        }

        [Fact]
        public void TokenResponse_WithNullValues_ShouldDeserialize()
        {
            // Arrange
            var json = @"{
                ""access_token"": ""token123"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600
            }";

            // Act
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

            // Assert
            Assert.NotNull(tokenResponse);
            Assert.Equal("token123", tokenResponse.AccessToken);
            Assert.Null(tokenResponse.RefreshToken);
            Assert.Null(tokenResponse.Scope);
        }

        [Fact]
        public void TokenResponse_WithEmptyJson_ShouldDeserializeToDefaultValues()
        {
            // Arrange
            var json = "{}";

            // Act
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

            // Assert
            Assert.NotNull(tokenResponse);
            Assert.Null(tokenResponse.AccessToken);
            Assert.Null(tokenResponse.TokenType);
            Assert.Equal(0, tokenResponse.ExpiresIn);
            Assert.Null(tokenResponse.RefreshToken);
            Assert.Null(tokenResponse.Scope);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(60)]
        [InlineData(3600)]
        [InlineData(7200)]
        [InlineData(86400)]
        public void TokenResponse_ExpiresIn_ShouldAcceptVariousValues(int expiresIn)
        {
            // Arrange & Act
            var tokenResponse = new TokenResponse
            {
                ExpiresIn = expiresIn
            };

            // Assert
            Assert.Equal(expiresIn, tokenResponse.ExpiresIn);
        }

        [Fact]
        public void TokenResponse_CompleteFortnoxResponse_ShouldDeserialize()
        {
            // Arrange - Simulating an actual Fortnox API response
            var json = @"{
                ""access_token"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"",
                ""token_type"": ""Bearer"",
                ""expires_in"": 3600,
                ""refresh_token"": ""def50200a1b2c3d4e5f6"",
                ""scope"": ""companyinformation customer invoice article""
            }";

            // Act
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(json);

            // Assert
            Assert.NotNull(tokenResponse);
            Assert.Equal("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", tokenResponse.AccessToken);
            Assert.Equal("Bearer", tokenResponse.TokenType);
            Assert.Equal(3600, tokenResponse.ExpiresIn);
            Assert.Equal("def50200a1b2c3d4e5f6", tokenResponse.RefreshToken);
            Assert.Equal("companyinformation customer invoice article", tokenResponse.Scope);
        }

        [Fact]
        public void TokenResponse_ExpiresAt_ShouldBeInFuture()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var expiresIn = 3600;

            // Act
            var tokenResponse = new TokenResponse
            {
                ExpiresIn = expiresIn,
                ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn)
            };

            // Assert
            Assert.True(tokenResponse.ExpiresAt > now);
        }

        [Fact]
        public void TokenResponse_JsonPropertyNames_ShouldBeSnakeCase()
        {
            // Arrange
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test",
                TokenType = "Bearer",
                ExpiresIn = 100,
                RefreshToken = "refresh",
                Scope = "scope"
            };

            // Act
            var json = JsonConvert.SerializeObject(tokenResponse);

            // Assert - Verify snake_case naming
            Assert.Contains("access_token", json);
            Assert.Contains("token_type", json);
            Assert.Contains("expires_in", json);
            Assert.Contains("refresh_token", json);
            Assert.DoesNotContain("AccessToken", json);
            Assert.DoesNotContain("TokenType", json);
            Assert.DoesNotContain("ExpiresIn", json);
            Assert.DoesNotContain("RefreshToken", json);
        }
    }
}
