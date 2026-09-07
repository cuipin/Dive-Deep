using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
