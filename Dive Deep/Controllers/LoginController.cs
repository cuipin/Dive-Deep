using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
