using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ContractModel = GLMS.Web.Models.Contract;


namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IContractRepository _contractRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IFileService _fileService;

        public ContractsController(
            IContractRepository contractRepository,
            IClientRepository clientRepository,
            IFileService fileService)
        {
            _contractRepository = contractRepository;
            _clientRepository = clientRepository;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            var contracts = await _contractRepository.SearchContractsAsync(startDate, endDate, status);

            // Store filter values for view
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.CurrentStatus = status;
            ViewBag.StatusList = Enum.GetValues(typeof(ContractStatus))
                .Cast<ContractStatus>()
                .Select(s => new SelectListItem { Value = s.ToString(), Text = s.ToString() });

            return View(contracts);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var clients = await _clientRepository.GetAllAsync();
            var viewModel = new ContractCreateViewModel
            {
                AvailableClients = clients.ToList(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Status = ContractStatus.Draft
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractCreateViewModel viewModel)
        {
            // Remove AvailableClients from ModelState validation
            ModelState.Remove("AvailableClients");

            System.Diagnostics.Debug.WriteLine("=== CREATE CONTRACT POST STARTED ===");
            System.Diagnostics.Debug.WriteLine($"ModelState IsValid: {ModelState.IsValid}");
            System.Diagnostics.Debug.WriteLine($"ClientId: {viewModel.ClientId}");
            System.Diagnostics.Debug.WriteLine($"ContractNumber: {viewModel.ContractNumber}");

            if (ModelState.IsValid)
            {
                string pdfPath = null;

                // Handle file upload
                if (viewModel.SignedAgreement != null)
                {
                    if (!_fileService.IsValidPdfFile(viewModel.SignedAgreement))
                    {
                        ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed.");
                        viewModel.AvailableClients = (await _clientRepository.GetAllAsync()).ToList();
                        return View(viewModel);
                    }

                    pdfPath = await _fileService.SavePdfFileAsync(
                        viewModel.SignedAgreement,
                        viewModel.ContractNumber);
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
                    PdfFilePath = pdfPath,
                    CreatedAt = DateTime.UtcNow
                };

                await _contractRepository.AddAsync(contract);
                TempData["Success"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If we get here, ModelState is invalid
            System.Diagnostics.Debug.WriteLine("ModelState is INVALID. Errors:");
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key]?.Errors;
                if (errors != null && errors.Any())
                {
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {key}: {error.ErrorMessage}");
                    }
                }
            }

            viewModel.AvailableClients = (await _clientRepository.GetAllAsync()).ToList();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);

            if (contract == null)
            {
                return NotFound("Contract not found.");
            }

            if (string.IsNullOrEmpty(contract.PdfFilePath))
            {
                return NotFound("No PDF file associated with this contract.");
            }

            try
            {
                // Get the full file path
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.PdfFilePath.TrimStart('/'));

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound("PDF file not found on server.");
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
                string fileName = Path.GetFileName(contract.PdfFilePath);

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return NotFound($"Error retrieving file: {ex.Message}");
            }
        }
    }
}
