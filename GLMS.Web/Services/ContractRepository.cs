using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace GLMS.Web.Services
{
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // LINQ Search/Filter implementation
        public async Task<IEnumerable<Contract>> SearchContractsAsync(
            DateTime? startDate,
            DateTime? endDate,
            ContractStatus? status)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query.OrderByDescending(c => c.StartDate).ToListAsync();
        }

        public async Task<IEnumerable<Contract>> FindByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Contracts
                .Where(c => c.StartDate >= start && c.EndDate <= end)
                .Include(c => c.Client)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> FindByStatusAsync(ContractStatus status)
        {
            return await _context.Contracts
                .Where(c => c.Status == status)
                .Include(c => c.Client)
                .ToListAsync();
        }

        public async Task<Contract> AddAsync(Contract contract)
        {
            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<Contract> UpdateAsync(Contract contract)
        {
            _context.Entry(contract).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var contract = await GetByIdAsync(id);
            if (contract == null)
                return false;

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ContractExistsAsync(int id)
        {
            return await _context.Contracts.AnyAsync(c => c.Id == id);
        }

        // CRITICAL: Validation for ServiceRequest creation
        public async Task<bool> IsContractActiveForRequestAsync(int contractId)
        {
            var contract = await GetByIdAsync(contractId);
            if (contract == null)
                return false;

            // Business rule: ServiceRequest cannot be created if Contract is Expired or On Hold
            return contract.Status != ContractStatus.Expired &&
                   contract.Status != ContractStatus.OnHold;
        }
    }
}