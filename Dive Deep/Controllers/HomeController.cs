using System.Diagnostics;
using Dive_Deep.Models;
using Dive_Deep.Services;
using Dive_Deep.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _products;

        public HomeController(ILogger<HomeController> logger, IProductService products)
        {
            _logger = logger;
            _products = products;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var featuredProducts = await _products.GetFeaturedProductsAsync(4, cancellationToken);
            return View(new HomePageViewModel { FeaturedProducts = featuredProducts });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
