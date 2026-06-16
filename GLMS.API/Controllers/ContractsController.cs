using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GLMS.Shared.Models;
using GLMS.API.Services;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly IContractRepository _contractRepository;
        private readonly IClientRepository _clientRepository;

        public ContractsController(
            IContractRepository contractRepository,
            IClientRepository clientRepository)
        {
            _contractRepository = contractRepository;
            _clientRepository = clientRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetContracts(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status)
        {
            ContractStatus? contractStatus = null;
            if (!string.IsNullOrEmpty(status))
            {
                contractStatus = Enum.Parse<ContractStatus>(status, true);
            }

            var contracts = await _contractRepository.SearchContractsAsync(startDate, endDate, contractStatus);
            return Ok(contracts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contract>> GetContract(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            return Ok(contract);
        }

        [HttpPost]
        public async Task<ActionResult<Contract>> CreateContract([FromBody] CreateContractRequest request)
        {
            try
            {
                Console.WriteLine("=== CREATE CONTRACT CALLED ===");

                // Validate required fields
                if (request.ClientId <= 0)
                {
                    return BadRequest(new { message = "ClientId is required" });
                }

                if (string.IsNullOrEmpty(request.ContractNumber))
                {
                    return BadRequest(new { message = "ContractNumber is required" });
                }

                // Check if client exists
                var client = await _clientRepository.GetByIdAsync(request.ClientId);
                if (client == null)
                {
                    return BadRequest(new { message = $"Client with ID {request.ClientId} not found" });
                }

                // Safely parse dates
                DateTime startDate;
                DateTime endDate;

                try
                {
                    startDate = request.StartDate;
                    endDate = request.EndDate;
                }
                catch
                {
                    // If conversion fails, try parsing from string
                    startDate = DateTime.Parse(request.StartDate.ToString());
                    endDate = DateTime.Parse(request.EndDate.ToString());
                }

                // Safely parse status
                ContractStatus contractStatus;
                if (request.Status is string statusString)
                {
                    contractStatus = Enum.Parse<ContractStatus>(statusString, true);
                }
                else
                {
                    contractStatus = ContractStatus.Draft;
                }

                var contract = new Contract
                {
                    ClientId = request.ClientId,
                    ContractNumber = request.ContractNumber,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = contractStatus,
                    ServiceLevel = request.ServiceLevel ?? "Standard",
                    TermsAndConditions = request.TermsAndConditions ?? string.Empty,
                    PdfFilePath = request.PdfFilePath ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _contractRepository.AddAsync(contract);

                return CreatedAtAction(nameof(GetContract), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in CreateContract: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateContractStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                return NotFound(new { message = $"Contract with ID {id} not found" });

            contract.Status = Enum.Parse<ContractStatus>(request.Status, true);
            await _contractRepository.UpdateAsync(contract);

            return Ok(new { id = contract.Id, status = contract.Status.ToString(), message = "Status updated successfully" });
        }

        [HttpGet("active/{contractId}")]
        public async Task<ActionResult<bool>> IsContractActive(int contractId)
        {
            var isValid = await _contractRepository.IsContractActiveForRequestAsync(contractId);
            return Ok(new { contractId, isValid, message = isValid ? "Contract is valid" : "Contract is expired or on hold" });
        }
    }

    public class CreateContractRequest
    {
        public int ClientId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string ServiceLevel { get; set; } = string.Empty;
        public string? TermsAndConditions { get; set; }
        public string? PdfFilePath { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}