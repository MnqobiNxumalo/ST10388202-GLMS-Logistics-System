using Microsoft.AspNetCore.Mvc;
using GLMS.Shared.Models;
using GLMS.Shared.ViewModels;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IApiService _apiService;

        public ContractsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            // Check if user is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var contracts = await _apiService.GetContractsAsync(startDate, endDate, status);

            // Pass filter values to view
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.CurrentStatus = status;
            ViewBag.StatusList = Enum.GetValues(typeof(ContractStatus))
                .Cast<ContractStatus>()
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                });

            return View(contracts);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var contract = await _apiService.GetContractByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            return View(contract);
        }

        // GET: Contracts/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var clients = await _apiService.GetClientsAsync();
            var viewModel = new ContractCreateViewModel
            {
                AvailableClients = clients,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Status = ContractStatus.Draft,
                ServiceLevel = "Gold"
            };
            return View(viewModel);
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractCreateViewModel viewModel)
        {
            // Remove AvailableClients from validation
            ModelState.Remove("AvailableClients");

            if (ModelState.IsValid)
            {
                string pdfPath = null;  // ← DECLARE THIS HERE!

                // Handle file upload
                if (viewModel.SignedAgreement != null)
                {
                    // Save the file and get the path
                    pdfPath = await SavePdfFile(viewModel.SignedAgreement, viewModel.ContractNumber);
                }

                var contract = new Contract
                {
                    ClientId = viewModel.ClientId,
                    ContractNumber = viewModel.ContractNumber,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    Status = viewModel.Status,
                    ServiceLevel = viewModel.ServiceLevel,
                    TermsAndConditions = viewModel.TermsAndConditions,
                    PdfFilePath = pdfPath,  // ← NOW pdfPath EXISTS!
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _apiService.CreateContractAsync(contract);
                TempData["Success"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }

            viewModel.AvailableClients = await _apiService.GetClientsAsync();
            return View(viewModel);
        }

        // Helper method to save PDF
        private async Task<string> SavePdfFile(IFormFile file, string contractNumber)
        {
            if (file == null || file.Length == 0)
                return null;

            // Create uploads folder if it doesn't exist
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{contractNumber}_{timestamp}.pdf";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database
            return $"/uploads/contracts/{fileName}";
        }

        // GET: Contracts/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var contract = await _apiService.GetContractByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            var clients = await _apiService.GetClientsAsync();

            var viewModel = new ContractEditViewModel
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                ContractNumber = contract.ContractNumber,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                ServiceLevel = contract.ServiceLevel,
                TermsAndConditions = contract.TermsAndConditions,
                AvailableClients = clients
            };

            return View(viewModel);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            ModelState.Remove("AvailableClients");

            if (ModelState.IsValid)
            {
                var contract = new Contract
                {
                    Id = viewModel.Id,
                    ClientId = viewModel.ClientId,
                    ContractNumber = viewModel.ContractNumber,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    Status = viewModel.Status,
                    ServiceLevel = viewModel.ServiceLevel,
                    TermsAndConditions = viewModel.TermsAndConditions
                };

                // Update through API
                var updated = await _apiService.UpdateContractAsync(contract);
                if (updated)
                {
                    TempData["Success"] = "Contract updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to update contract.";
            }

            viewModel.AvailableClients = await _apiService.GetClientsAsync();
            return View(viewModel);
        }

        // POST: Contracts/UpdateStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, ContractStatus status)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return Unauthorized();
            }

            var result = await _apiService.UpdateContractStatusAsync(id, status);
            if (result)
            {
                TempData["Success"] = $"Contract status updated to {status}";
            }
            else
            {
                TempData["Error"] = "Failed to update contract status";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var contract = await _apiService.GetContractByIdAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.PdfFilePath))
            {
                TempData["Error"] = "No PDF file found for this contract.";
                return RedirectToAction(nameof(Index));
            }

            // In Part 3, PDFs should be served through API
            var fileBytes = await _apiService.DownloadPdfAsync(id);
            if (fileBytes == null)
            {
                TempData["Error"] = "PDF file not found on server.";
                return RedirectToAction(nameof(Index));
            }

            string fileName = Path.GetFileName(contract.PdfFilePath);
            return File(fileBytes, "application/pdf", fileName);
        }
    }

}