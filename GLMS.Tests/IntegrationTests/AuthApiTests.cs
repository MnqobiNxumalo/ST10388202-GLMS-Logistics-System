using Xunit;

namespace GLMS.Tests.IntegrationTests
{
    public class AuthApiTests : IntegrationTestBase
    {
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginData = new { username = "admin", password = "password" };
            var content = GetJsonContent(loginData);

            // Act
            var response = await _client.PostAsync("api/Auth/login", content);
            var json = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("token", json);
            Assert.Contains("admin", json);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginData = new { username = "wrong", password = "wrong" };
            var content = GetJsonContent(loginData);

            // Act
            var response = await _client.PostAsync("api/Auth/login", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            // Act - no authentication
            var response = await _client.GetAsync("api/Contracts");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithValidToken_ReturnsOk()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/Contracts");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}