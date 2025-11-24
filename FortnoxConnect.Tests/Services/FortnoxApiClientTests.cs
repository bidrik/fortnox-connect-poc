using System.Net;
using System.Net.Http.Headers;
using FortnoxConnect.Services;
using Moq;
using Moq.Protected;
using Xunit;

namespace FortnoxConnect.Tests.Services
{
    /// <summary>
    /// Unit tests for FortnoxApiClient
    /// </summary>
    public class FortnoxApiClientTests
    {
        private const string TestApiBaseUrl = "https://api.fortnox.se/3";
        private const string TestAccessToken = "test_access_token_12345";

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FortnoxApiClient(null!, TestApiBaseUrl));
        }

        [Fact]
        public void Constructor_WithNullApiBaseUrl_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FortnoxApiClient(httpClient, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldNotThrow()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public async Task GetAsync_WithNullEndpoint_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                client.GetAsync(null!, TestAccessToken));
        }

        [Fact]
        public async Task GetAsync_WithEmptyEndpoint_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                client.GetAsync(string.Empty, TestAccessToken));
        }

        [Fact]
        public async Task GetAsync_WithNullAccessToken_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                client.GetAsync("companyinformation", null!));
        }

        [Fact]
        public async Task GetAsync_WithEmptyAccessToken_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                client.GetAsync("companyinformation", string.Empty));
        }

        [Fact]
        public async Task GetAsync_WithValidParameters_ShouldReturnResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = @"{""CompanyInformation"": {""CompanyName"": ""Test Company""}}";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            var result = await client.GetAsync("companyinformation", TestAccessToken);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task GetAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

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
                    Content = new StringContent("{}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            await client.GetAsync("customers", TestAccessToken);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.NotNull(capturedRequest.Headers.Authorization);
            Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
            Assert.Equal(TestAccessToken, capturedRequest.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task GetAsync_ShouldSetAcceptHeader()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

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
                    Content = new StringContent("{}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            await client.GetAsync("invoices", TestAccessToken);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Contains(capturedRequest.Headers.Accept, 
                a => a.MediaType == "application/json");
        }

        [Fact]
        public async Task GetAsync_ShouldConstructCorrectUrl()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

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
                    Content = new StringContent("{}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            await client.GetAsync("test-endpoint", TestAccessToken);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal($"{TestApiBaseUrl}/test-endpoint", capturedRequest.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetAsync_WithHttpError_ShouldThrowHttpRequestException()
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
                    Content = new StringContent("Unauthorized")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                client.GetAsync("companyinformation", TestAccessToken));
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetAsync_WithVariousHttpErrors_ShouldThrowHttpRequestException(HttpStatusCode statusCode)
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
                    StatusCode = statusCode,
                    Content = new StringContent("Error")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
                client.GetAsync("endpoint", TestAccessToken));
            Assert.Contains(statusCode.ToString(), exception.Message);
        }

        [Fact]
        public async Task PostAsync_WithValidParameters_ShouldReturnResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = @"{""Customer"": {""CustomerNumber"": ""123""}}";
            var requestBody = @"{""Customer"": {""Name"": ""New Customer""}}";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            var result = await client.PostAsync("customers", TestAccessToken, requestBody);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task PostAsync_WithNullEndpoint_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.PostAsync(null!, TestAccessToken, "{}"));
        }

        [Fact]
        public async Task PostAsync_WithNullAccessToken_ShouldThrowArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.PostAsync("customers", null!, "{}"));
        }

        [Fact]
        public async Task PostAsync_ShouldSendJsonContent()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var requestBody = @"{""test"": ""data""}";

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
                    Content = new StringContent("{}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            await client.PostAsync("endpoint", TestAccessToken, requestBody);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest.Method);
            var content = await capturedRequest.Content!.ReadAsStringAsync();
            Assert.Equal(requestBody, content);
        }

        [Fact]
        public async Task PutAsync_WithValidParameters_ShouldReturnResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = @"{""Customer"": {""CustomerNumber"": ""123""}}";
            var requestBody = @"{""Customer"": {""Name"": ""Updated Customer""}}";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            var result = await client.PutAsync("customers/123", TestAccessToken, requestBody);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task PutAsync_ShouldUsePutMethod()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

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
                    Content = new StringContent("{}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            await client.PutAsync("customers/1", TestAccessToken, "{}");

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Put, capturedRequest.Method);
        }

        [Fact]
        public async Task DeleteAsync_WithValidParameters_ShouldReturnTrue()
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
                    StatusCode = HttpStatusCode.NoContent
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            var result = await client.DeleteAsync("customers/123", TestAccessToken);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldUseDeleteMethod()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            await client.DeleteAsync("customers/1", TestAccessToken);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Delete, capturedRequest.Method);
        }

        [Fact]
        public async Task DeleteAsync_WithHttpError_ShouldThrowHttpRequestException()
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
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Not found")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                client.DeleteAsync("customers/999", TestAccessToken));
        }

        [Fact]
        public async Task GetCompanyInformationAsync_ShouldCallCorrectEndpoint()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = @"{""CompanyInformation"": {}}";

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
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            var result = await client.GetCompanyInformationAsync(TestAccessToken);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Contains("companyinformation", capturedRequest.RequestUri?.ToString());
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldCallCorrectEndpoint()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = @"{""Customers"": []}";

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
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            var result = await client.GetCustomersAsync(TestAccessToken);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Contains("customers", capturedRequest.RequestUri?.ToString());
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task GetInvoicesAsync_ShouldCallCorrectEndpoint()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = @"{""Invoices"": []}";

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
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            var result = await client.GetInvoicesAsync(TestAccessToken);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Contains("invoices", capturedRequest.RequestUri?.ToString());
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task PostAsync_WithNullBody_ShouldSendEmptyString()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

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
                    Content = new StringContent("{}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var client = new FortnoxApiClient(httpClient, TestApiBaseUrl);

            // Act
            await client.PostAsync("endpoint", TestAccessToken, null!);

            // Assert
            Assert.NotNull(capturedRequest);
            var content = await capturedRequest.Content!.ReadAsStringAsync();
            Assert.Equal(string.Empty, content);
        }
    }
}
