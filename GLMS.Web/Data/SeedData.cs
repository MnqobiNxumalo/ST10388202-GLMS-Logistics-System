using GLMS.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if already seeded
            if (context.Clients.Any())
            {
                Console.WriteLine("Database already has data. Skipping seed.");
                return;
            }

            Console.WriteLine("Seeding database...");

            // Add sample clients
            var clients = new List<Client>
            {
                new Client
                {
                    Name = "TechMove Logistics SA",
                    Email = "contact@techmove.co.za",
                    PhoneNumber = "+27 11 234 5678",
                    Address = "123 Main Street, Johannesburg, 2000",
                    Region = "EMEA"
                },
                new Client
                {
                    Name = "Global Freight Solutions",
                    Email = "info@globalfreight.com",
                    PhoneNumber = "+1 555 123 4567",
                    Address = "456 Harbor Drive, New York, NY 10001",
                    Region = "NA"
                },
                new Client
                {
                    Name = "Asia Cargo Express",
                    Email = "dispatch@asiacargo.com",
                    PhoneNumber = "+65 6789 1234",
                    Address = "789 Shipping Lane, Singapore, 018989",
                    Region = "APAC"
                }
            };
            context.Clients.AddRange(clients);
            await context.SaveChangesAsync();

            // Add sample contracts
            var contracts = new List<Contract>
            {
                new Contract
                {
                    ClientId = 1,
                    ContractNumber = "GLMS-2024-001",
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 12, 31),
                    Status = ContractStatus.Active,
                    ServiceLevel = "Gold",
                    TermsAndConditions = "Standard terms for TechMove Logistics",
                    CreatedAt = DateTime.UtcNow
                },
                new Contract
                {
                    ClientId = 1,
                    ContractNumber = "GLMS-2024-002",
                    StartDate = new DateTime(2024, 3, 15),
                    EndDate = new DateTime(2025, 3, 14),
                    Status = ContractStatus.Active,
                    ServiceLevel = "Platinum",
                    TermsAndConditions = "Premium service agreement",
                    CreatedAt = DateTime.UtcNow
                },
                new Contract
                {
                    ClientId = 2,
                    ContractNumber = "GFS-2023-089",
                    StartDate = new DateTime(2023, 6, 1),
                    EndDate = new DateTime(2024, 5, 31),
                    Status = ContractStatus.Expired,
                    ServiceLevel = "Silver",
                    TermsAndConditions = "Standard freight agreement",
                    CreatedAt = DateTime.UtcNow
                },
                new Contract
                {
                    ClientId = 3,
                    ContractNumber = "ACE-2024-045",
                    StartDate = new DateTime(2024, 2, 1),
                    EndDate = new DateTime(2024, 8, 31),
                    Status = ContractStatus.OnHold,
                    ServiceLevel = "Bronze",
                    TermsAndConditions = "Temporary hold due to compliance review",
                    CreatedAt = DateTime.UtcNow
                }
            };
            context.Contracts.AddRange(contracts);
            await context.SaveChangesAsync();

            Console.WriteLine($"Added {clients.Count} clients and {contracts.Count} contracts.");
        }
    }
}