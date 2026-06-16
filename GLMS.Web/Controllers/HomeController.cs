using Microsoft.AspNetCore.Mvc;
using GLMS.Shared.ViewModels;

namespace GLMS.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect to login if not authenticated
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
