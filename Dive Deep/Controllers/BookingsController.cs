using Dive_Deep.Data;
using Dive_Deep.Persistence;
using Dive_Deep.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    public class BookingsController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(
            IBookingRepository bookingRepository,
            UserManager<ApplicationUser> userManager)
        {
            _bookingRepository = bookingRepository;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);

            var bookings = _bookingRepository.GetAll()
                .Where(b => b.UserId == userId)
                .ToList();

            return View(bookings);
        }

        public IActionResult Add(int? id)
        {
            ViewBag.Action = "add";

            var bookingVM = new BookingViewModel
            {
                Products = ProductRepository.GetAll()
            };

            var date = DateTime.Now;

            bookingVM.Booking.StartTime = new DateTime(
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute,
                0);

            bookingVM.Booking.EndTime = bookingVM.Booking.StartTime.AddHours(1);

            if (id != null)
            {
                bookingVM.Booking.ProductId = id.Value;
            }

            return View("BookingView", bookingVM);
        }

        [HttpPost]
        public IActionResult Add(BookingViewModel bookingVM)
        {
            bookingVM.Booking.UserId = _userManager.GetUserId(User);

            ModelState.Remove("Booking.UserId");

            if (!ModelState.IsValid)
            {
                bookingVM.Products = ProductRepository.GetAll();
                ViewBag.Action = "add";

                return View("BookingView", bookingVM);
            }

            _bookingRepository.Add(bookingVM.Booking);

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? id)
        {
            var booking = _bookingRepository.GetById(id ?? 0);

            if (booking == null)
            {
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);

            if (booking.UserId != userId)
            {
                return RedirectToAction("Index");
            }

            var bookingVM = new BookingViewModel
            {
                Booking = booking,
                Products = ProductRepository.GetAll()
            };

            ViewBag.Action = "edit";

            return View("BookingView", bookingVM);
        }

        [HttpPost]
        public IActionResult Edit(BookingViewModel bookingVM)
        {
            var existing = _bookingRepository.GetById(
                bookingVM.Booking.BookingId);

            if (existing == null)
            {
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);

            if (existing.UserId != userId)
            {
                return RedirectToAction("Index");
            }

            bookingVM.Booking.UserId = userId;

            ModelState.Remove("Booking.UserId");

            if (!ModelState.IsValid)
            {
                bookingVM.Products = ProductRepository.GetAll();
                ViewBag.Action = "edit";

                return View("BookingView", bookingVM);
            }

            _bookingRepository.Update(bookingVM.Booking);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var booking = _bookingRepository.GetById(id);

            if (booking == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (booking.UserId != userId)
            {
                return Forbid();
            }

            _bookingRepository.Delete(id);

            return RedirectToAction("Index");
        }
    }
}