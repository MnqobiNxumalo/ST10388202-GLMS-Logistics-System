using Microsoft.AspNetCore.Mvc;
using GLMS.Shared.Models;
using GLMS.Shared.ViewModels;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly IApiService _apiService;

        public ServiceRequestController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _apiService.GetServiceRequestsAsync();
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int contractId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var contract = await _apiService.GetContractByIdAsync(contractId);
            if (contract == null)
            {
                return NotFound();
            }

            if (!contract.CanCreateServiceRequest())
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
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Check if contract is valid
                var contract = await _apiService.GetContractByIdAsync(model.ContractId);
                if (contract == null || !contract.CanCreateServiceRequest())
                {
                    ModelState.AddModelError("", "Cannot create service request for expired or on-hold contracts.");
                    return View(model);
                }

                // Get exchange rate and convert
                decimal zarAmount = await _apiService.ConvertCurrencyAsync(model.AmountUSD);
                decimal rate = 19.50m; // You can get this from API response

                var request = new ServiceRequest
                {
                    ContractId = model.ContractId,
                    Description = model.Description,
                    AmountUSD = model.AmountUSD,
                    AmountZAR = zarAmount,
                    ExchangeRateUsed = rate,
                    Status = RequestStatus.Pending,
                    Notes = model.Notes,
                    RequestDate = DateTime.UtcNow
                };

                var created = await _apiService.CreateServiceRequestAsync(request);
                TempData["Success"] = "Service request created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _apiService.GetServiceRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> GetZarAmount(decimal usdAmount)
        {
            var zarAmount = await _apiService.ConvertCurrencyAsync(usdAmount);
            return Json(new { zarAmount, rate = 19.50m });
        }
    }
}