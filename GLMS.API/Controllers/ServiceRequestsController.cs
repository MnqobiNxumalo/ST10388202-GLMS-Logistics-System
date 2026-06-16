using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GLMS.Shared.Models;
using GLMS.API.Services;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly IContractRepository _contractRepository;

        public ServiceRequestsController(
            IServiceRequestRepository serviceRequestRepository,
            IContractRepository contractRepository)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _contractRepository = contractRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests()
        {
            try
            {
                var requests = await _serviceRequestRepository.GetAllAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequest>> GetServiceRequest(int id)
        {
            try
            {
                var request = await _serviceRequestRepository.GetByIdAsync(id);
                if (request == null)
                    return NotFound();
                return Ok(request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ServiceRequest>> CreateServiceRequest([FromBody] CreateServiceRequestRequest request)
        {
            try
            {
                Console.WriteLine("=== CREATE SERVICE REQUEST CALLED ===");
                Console.WriteLine($"ContractId: {request.ContractId}");
                Console.WriteLine($"Description: {request.Description}");
                Console.WriteLine($"AmountUSD: {request.AmountUSD}");

                // Get the contract
                var contract = await _contractRepository.GetByIdAsync(request.ContractId);

                if (contract == null)
                {
                    Console.WriteLine("ERROR: Contract not found");
                    return BadRequest(new { message = $"Contract with ID {request.ContractId} not found" });
                }

                Console.WriteLine($"Contract Status: {contract.Status}");
                Console.WriteLine($"Contract Client: {contract.Client?.Name}");

                // Check if contract is valid for service requests
                if (contract.Status == ContractStatus.Expired)
                {
                    Console.WriteLine("REJECTED: Contract is Expired");
                    return BadRequest(new { message = "Cannot create service request for expired contract" });
                }

                if (contract.Status == ContractStatus.OnHold)
                {
                    Console.WriteLine("REJECTED: Contract is On Hold");
                    return BadRequest(new { message = "Cannot create service request for on-hold contract" });
                }

                // Check if contract is active by date
                if (contract.StartDate > DateTime.Today)
                {
                    Console.WriteLine($"REJECTED: Contract starts on {contract.StartDate}");
                    return BadRequest(new { message = "Contract has not started yet" });
                }

                if (contract.EndDate < DateTime.Today)
                {
                    Console.WriteLine($"REJECTED: Contract ended on {contract.EndDate}");
                    return BadRequest(new { message = "Contract has already expired" });
                }

                Console.WriteLine("Contract is valid - proceeding");

                var serviceRequest = new ServiceRequest
                {
                    ContractId = request.ContractId,
                    Description = request.Description,
                    AmountUSD = request.AmountUSD,
                    AmountZAR = request.AmountUSD * 19.50m,
                    ExchangeRateUsed = 19.50m,
                    Status = RequestStatus.Pending,
                    Notes = request.Notes ?? string.Empty,
                    RequestDate = DateTime.UtcNow
                };

                var created = await _serviceRequestRepository.AddAsync(serviceRequest);
                Console.WriteLine($"Created Service Request with ID: {created.Id}");

                return CreatedAtAction(nameof(GetServiceRequest), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"STACK: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("currency/convert")]
        public async Task<IActionResult> ConvertCurrency([FromQuery] decimal usdAmount)
        {
            var rate = 19.50m;
            var zarAmount = usdAmount * rate;
            return Ok(new { usdAmount, zarAmount, rate });
        }
    }

    public class CreateServiceRequestRequest
    {
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal AmountUSD { get; set; }
        public string? Notes { get; set; }
    }
}