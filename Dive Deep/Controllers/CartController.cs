using Dive_Deep.Data;
using Dive_Deep.Models;
using Dive_Deep.Persistence;
using Dive_Deep.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(IBookingRepository bookingRepository, UserManager<ApplicationUser> userManager)
        {
            _bookingRepository = bookingRepository;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var productIds = CartService.GetCart(HttpContext);

            var items = productIds
                .Select(id => ProductRepository.GetById(id))
                .Where(p => p != null)
                .Select(p => new CartItemViewModel
                {
                    ProductId = p!.ProductId,
                    DisplayName = p.DisplayName,
                    PrisPrDag = p.PrisPrDag
                })
                .ToList();

            return View(items);
        }

        [HttpPost]
        public IActionResult Add(int productId)
        {
            CartService.AddToCart(HttpContext, productId);

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Remove(int index)
        {
            CartService.RemoveAt(HttpContext, index);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Confirm(List<CartItemViewModel> items)
        {
            var userId = _userManager.GetUserId(User);

            foreach (var item in items)
            {
                var booking = new Booking
                {
                    ProductId = item.ProductId,
                    UserId = userId!,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime
                };

                _bookingRepository.Add(booking);
            }

            CartService.Clear(HttpContext);

            return RedirectToAction("Index", "Bookings");
        }
    }
}