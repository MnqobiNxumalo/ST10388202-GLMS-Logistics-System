using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly IServiceRequestRepository _serviceRequestRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestController(
            IServiceRequestRepository serviceRequestRepository,
            IContractRepository contractRepository,
            ICurrencyService currencyService)
        {
            _serviceRequestRepository = serviceRequestRepository;
            _contractRepository = contractRepository;
            _currencyService = currencyService;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _serviceRequestRepository.GetAllAsync();
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int contractId)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                return NotFound();
            }

            

                // Check if contract is valid for service requests
                if (!await _contractRepository.IsContractActiveForRequestAsync(contractId))
            {
                TempData["Error"] = "Cannot create service request for expired or on-hold contracts.";
                return RedirectToAction("Index", "Contracts");
            }

            var viewModel = new ServiceRequestViewModel
            {
                ContractId = contractId,
                ContractDisplayName = $"{contract.ContractNumber} - {contract.Client?.Name}"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel model)
        {
            ModelState.Remove("ContractDisplayName");


            if (ModelState.IsValid)
            {
                // VALIDATION: Check if contract is active for service request
                bool isContractValid = await _contractRepository.IsContractActiveForRequestAsync(model.ContractId);

                if (!isContractValid)
                {
                    ModelState.AddModelError("ContractId",
                        "Service requests cannot be created for contracts that are Expired or On Hold.");

                    var contract = await _contractRepository.GetByIdAsync(model.ContractId);
                    model.ContractDisplayName = contract?.ContractNumber;
                    return View(model);
                }

                // Get current exchange rate
                decimal exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                decimal zarAmount = await _currencyService.ConvertUsdToZarAsync(model.AmountUSD);

                var serviceRequest = new ServiceRequest
                {
                    ContractId = model.ContractId,
                    Description = model.Description,
                    AmountUSD = model.AmountUSD,
                    AmountZAR = zarAmount,
                    ExchangeRateUsed = exchangeRate,
                    Status = RequestStatus.Pending,
                    Notes = model.Notes,
                    RequestDate = DateTime.UtcNow
                };

                await _serviceRequestRepository.AddAsync(serviceRequest);
                TempData["Success"] = "Service request created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate contract display name if validation fails
            var existingContract = await _contractRepository.GetByIdAsync(model.ContractId);
            model.ContractDisplayName = existingContract?.ContractNumber;

            return View(model);
        }

        // AJAX endpoint for real-time currency conversion
        [HttpGet]
        public async Task<IActionResult> GetZarAmount(decimal usdAmount)
        {
            if (usdAmount <= 0)
                return Json(new { zarAmount = 0, rate = 0 });

            decimal rate = await _currencyService.GetUsdToZarRateAsync();
            decimal zarAmount = usdAmount * rate;

            return Json(new
            {
                zarAmount = Math.Round(zarAmount, 2),
                rate = Math.Round(rate, 4)
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _serviceRequestRepository.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            return View(request);
        }
    }
}