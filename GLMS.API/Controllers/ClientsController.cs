using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GLMS.Shared.Models;
using GLMS.API.Services;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;

        public ClientsController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            var clients = await _clientRepository.GetAllAsync();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                return NotFound();
            return Ok(client);
        }

        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient([FromBody] Client client)
        {
            var created = await _clientRepository.AddAsync(client);
            return CreatedAtAction(nameof(GetClient), new { id = created.Id }, created);
        }
    }
}