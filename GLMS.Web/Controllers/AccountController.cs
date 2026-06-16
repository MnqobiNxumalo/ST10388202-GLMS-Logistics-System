using Microsoft.AspNetCore.Mvc;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var token = await _apiService.LoginAsync(username, password);
            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Contracts");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JWToken");
            return RedirectToAction("Login");
        }
    }
}