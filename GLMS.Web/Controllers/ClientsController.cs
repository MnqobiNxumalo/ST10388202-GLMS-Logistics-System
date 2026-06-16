using Microsoft.AspNetCore.Mvc;
using GLMS.Shared.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IApiService _apiService;

        public ClientsController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var clients = await _apiService.GetClientsAsync();
            return View(clients);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                await _apiService.CreateClientAsync(client);
                TempData["Success"] = "Client created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = await _apiService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }
    }
}