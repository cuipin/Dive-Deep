using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Dive_Deep.Models;
using Dive_Deep.Persistence;
using Dive_Deep.Models.ProduktTypes;

namespace Dive_Deep.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BCD_Products()
        {
            var bcds = ProductRepository.GetAllOfType<BCD>().ToList();
            return View("BCD_Products", bcds);
        }

        public IActionResult Dykkedragter_Products()
        {
            var dykkedragter = ProductRepository.GetAllOfType<Dykkerdragter>().ToList();
            return View("Dykkedragter_Products", dykkedragter);
        }
    }
}
