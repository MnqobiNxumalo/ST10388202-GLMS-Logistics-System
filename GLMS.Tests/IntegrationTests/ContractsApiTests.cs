using Xunit;
using GLMS.Shared.Models;

namespace GLMS.Tests.IntegrationTests
{
    public class ContractsApiTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetContracts_ReturnsOkStatusCode()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/Contracts");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_ReturnsNonNullData()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/Contracts");
            var json = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.NotNull(json);
            Assert.NotEmpty(json);
        }

        [Fact]
        public async Task GetContracts_ReturnsValidJsonArray()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/Contracts");
            var contracts = await DeserializeAsync<List<Contract>>(response);

            // Assert
            Assert.NotNull(contracts);
            Assert.IsType<List<Contract>>(contracts);
        }

        [Fact]
        public async Task GetContractById_WithValidId_ReturnsOk()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/Contracts/1");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetContractById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/Contracts/99999");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateContract_WithValidData_ReturnsCreated()
        {
            // Arrange
            await AuthenticateAsync();
            var newContract = new
            {
                clientId = 1,
                contractNumber = $"TEST-{DateTime.Now.Ticks}",
                startDate = DateTime.Today.ToString("yyyy-MM-dd"),
                endDate = DateTime.Today.AddYears(1).ToString("yyyy-MM-dd"),
                status = "Active",
                serviceLevel = "Gold"
            };
            var content = GetJsonContent(newContract);

            // Act
            var response = await _client.PostAsync("api/Contracts", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateContract_WithMissingClientId_ReturnsBadRequest()
        {
            // Arrange
            await AuthenticateAsync();
            var invalidContract = new
            {
                contractNumber = "INVALID-TEST",
                status = "Active"
            };
            var content = GetJsonContent(invalidContract);

            // Act
            var response = await _client.PostAsync("api/Contracts", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateContractStatus_ReturnsOk()
        {
            // Arrange
            await AuthenticateAsync();
            var updateData = new { status = "Active" };
            var content = GetJsonContent(updateData);

            // Act
            var response = await _client.PatchAsync("api/Contracts/1/status", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_WithStatusFilter_ReturnsFilteredResults()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/Contracts?status=Active");
            var contracts = await DeserializeAsync<List<Contract>>(response);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            if (contracts != null)
            {
                Assert.All(contracts, c => Assert.Equal(ContractStatus.Active, c.Status));
            }
        }

        [Fact]
        public async Task GetContracts_WithDateRangeFilter_ReturnsOk()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await _client.GetAsync("api/Contracts?startDate=2024-01-01&endDate=2024-12-31");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}