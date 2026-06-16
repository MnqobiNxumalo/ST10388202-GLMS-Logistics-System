using Xunit;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Shared.Models;
using GLMS.Web.Services;

namespace GLMS.Tests
{
    public class ContractRepositoryTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task SearchContractsAsync_ByStatus_ReturnsFilteredContracts()
        {
            var options = CreateNewContextOptions();

            using var context = new ApplicationDbContext(options);
            var repository = new ContractRepository(context);

            // Seed data
            var client = new Client
            {
                Name = "Test Client",
                Email = "test@test.com",
                Region = "NA",
                Address = "123 Test St",
                PhoneNumber = "123456789"
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            context.Contracts.AddRange(
                new Contract { ClientId = 1, ContractNumber = "C001", Status = ContractStatus.Active, StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1), ServiceLevel = "Gold", CreatedAt = DateTime.UtcNow },
                new Contract { ClientId = 1, ContractNumber = "C002", Status = ContractStatus.Expired, StartDate = DateTime.Today.AddYears(-2), EndDate = DateTime.Today.AddYears(-1), ServiceLevel = "Silver", CreatedAt = DateTime.UtcNow },
                new Contract { ClientId = 1, ContractNumber = "C003", Status = ContractStatus.Active, StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1), ServiceLevel = "Gold", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var activeContracts = await repository.FindByStatusAsync(ContractStatus.Active);
            var expiredContracts = await repository.FindByStatusAsync(ContractStatus.Expired);

            Assert.Equal(2, activeContracts.Count());
            Assert.Single(expiredContracts);
        }

        [Fact]
        public async Task SearchContractsAsync_ByDateRange_ReturnsCorrectContracts()
        {
            var options = CreateNewContextOptions();

            using var context = new ApplicationDbContext(options);
            var repository = new ContractRepository(context);

            var client = new Client
            {
                Name = "Test Client",
                Email = "test@test.com",
                Region = "NA",
                Address = "123 Test St",
                PhoneNumber = "123456789"
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            context.Contracts.AddRange(
                new Contract { ClientId = 1, ContractNumber = "C001", Status = ContractStatus.Active, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 12, 31), ServiceLevel = "Gold", CreatedAt = DateTime.UtcNow },
                new Contract { ClientId = 1, ContractNumber = "C002", Status = ContractStatus.Active, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 12, 31), ServiceLevel = "Silver", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var contracts = await repository.FindByDateRangeAsync(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

            Assert.Single(contracts);
            Assert.Equal("C001", contracts.First().ContractNumber);
        }

        [Fact]
        public async Task IsContractActiveForRequestAsync_WithExpiredContract_ReturnsFalse()
        {
            var options = CreateNewContextOptions();

            using var context = new ApplicationDbContext(options);
            var repository = new ContractRepository(context);

            var client = new Client
            {
                Name = "Test Client",
                Email = "test@test.com",
                Region = "NA",
                Address = "123 Test St",
                PhoneNumber = "123456789"
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var expiredContract = new Contract
            {
                ClientId = 1,
                ContractNumber = "EXP001",
                Status = ContractStatus.Expired,
                StartDate = DateTime.Today.AddYears(-2),
                EndDate = DateTime.Today.AddDays(-1),
                ServiceLevel = "Bronze",
                CreatedAt = DateTime.UtcNow
            };
            context.Contracts.Add(expiredContract);
            await context.SaveChangesAsync();

            bool result = await repository.IsContractActiveForRequestAsync(expiredContract.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task IsContractActiveForRequestAsync_WithActiveContract_ReturnsTrue()
        {
            var options = CreateNewContextOptions();

            using var context = new ApplicationDbContext(options);
            var repository = new ContractRepository(context);

            var client = new Client
            {
                Name = "Test Client",
                Email = "test@test.com",
                Region = "NA",
                Address = "123 Test St",
                PhoneNumber = "123456789"
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var activeContract = new Contract
            {
                ClientId = 1,
                ContractNumber = "ACT001",
                Status = ContractStatus.Active,
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today.AddDays(10),
                ServiceLevel = "Gold",
                CreatedAt = DateTime.UtcNow
            };
            context.Contracts.Add(activeContract);
            await context.SaveChangesAsync();

            bool result = await repository.IsContractActiveForRequestAsync(activeContract.Id);

            Assert.True(result);
        }
    }
}