using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
