using Xunit;
using GLMS.Shared.Models;

namespace GLMS.Tests.IntegrationTests
{
    public class ServiceRequestsApiTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetServiceRequests_ReturnsOkStatusCode()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/ServiceRequests");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetServiceRequests_ReturnsNonNullData()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/ServiceRequests");
            var json = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.NotNull(json);
        }

        [Fact]
        public async Task GetServiceRequestById_WithValidId_ReturnsOk()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/ServiceRequests/1");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetServiceRequestById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/ServiceRequests/99999");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateServiceRequest_WithValidData_ReturnsCreated()
        {
            await AuthenticateAsync();
            var newRequest = new
            {
                contractId = 1,  // Make sure this contract is ACTIVE
                description = "Integration Test Request",
                amountUSD = 500.00m,
                notes = "Created by integration test"
            };
            var content = GetJsonContent(newRequest);
            var response = await _client.PostAsync("api/ServiceRequests", content);

            // Debug: Print the actual response
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateServiceRequest_ForExpiredContract_ReturnsBadRequest()
        {
            await AuthenticateAsync();

            // Make sure this contract ID is actually expired in your database
            // Check which ID has Status = 'Expired'
            int expiredContractId = 2;  // Change this to an actual expired contract ID

            var newRequest = new
            {
                contractId = expiredContractId,
                description = "Test on expired contract",
                amountUSD = 100.00m,
                notes = "Should fail"
            };

            var content = GetJsonContent(newRequest);
            var response = await _client.PostAsync("api/ServiceRequests", content);

            // This should be BadRequest, not Created
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ConvertCurrency_WithValidAmount_ReturnsZarAmount()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/ServiceRequests/currency/convert?usdAmount=100");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("zarAmount", json);
            Assert.Contains("usdAmount", json);
        }

        [Fact]
        public async Task ConvertCurrency_WithZeroAmount_ReturnsZero()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/ServiceRequests/currency/convert?usdAmount=0");
            var result = await DeserializeAsync<CurrencyResponse>(response);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            if (result != null)
            {
                Assert.Equal(0, result.zarAmount);
            }
        }

        private class CurrencyResponse
        {
            public decimal usdAmount { get; set; }
            public decimal zarAmount { get; set; }
            public decimal rate { get; set; }
        }
    }
}