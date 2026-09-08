using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
