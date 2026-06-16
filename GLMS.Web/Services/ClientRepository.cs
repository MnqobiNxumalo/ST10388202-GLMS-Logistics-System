using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Shared.Models;

namespace GLMS.Web.Services
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Client> GetByIdAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Client> AddAsync(Client client)
        {
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<Client> UpdateAsync(Client client)
        {
            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = await GetByIdAsync(id);
            if (client == null)
                return false;

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClientExistsAsync(int id)
        {
            return await _context.Clients.AnyAsync(c => c.Id == id);
        }
    }
}