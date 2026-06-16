using Microsoft.EntityFrameworkCore;
using GLMS.API.Data;
using GLMS.Shared.Models;

namespace GLMS.API.Services
{
    public class ServiceRequestRepository : IServiceRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceRequest> GetByIdAsync(int id)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .ThenInclude(c => c.Client)
                .OrderByDescending(sr => sr.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> GetByContractIdAsync(int contractId)
        {
            return await _context.ServiceRequests
                .Where(sr => sr.ContractId == contractId)
                .Include(sr => sr.Contract)
                .OrderByDescending(sr => sr.RequestDate)
                .ToListAsync();
        }

        public async Task<ServiceRequest> AddAsync(ServiceRequest request)
        {
            await _context.ServiceRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<ServiceRequest> UpdateAsync(ServiceRequest request)
        {
            _context.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var request = await GetByIdAsync(id);
            if (request == null)
                return false;

            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}