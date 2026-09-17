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

        public IActionResult Finner_Products()
        {
            var finner = ProductRepository.GetAllOfType<Finner>().ToList();
            return View("Finner_Products", finner);
        }

        public IActionResult MaskeSnorkel_Products()
        {
            var maskeSnorkel = ProductRepository.GetAllOfType<MaskeSnorkel>().ToList();
            return View("MaskeSnorkel_Products", maskeSnorkel);
        }

        public IActionResult Regulatorsæt_Products()
        {
            var regulatorsæt = ProductRepository.GetAllOfType<Regulatorsæt>().ToList();
            return View("Regulatorsæt_Products", regulatorsæt);
        }

        public IActionResult Tanke_Products()
        {
            var tanke = ProductRepository.GetAllOfType<Tanke>().ToList();
            return View("Tanke_Products", tanke);
        }
    }
}
