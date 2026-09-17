using Dive_Deep.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Dive_Deep.Models;
using Microsoft.AspNetCore.Mvc;
using Dive_Deep.ViewModels;
using Dive_Deep.Data;
using Dive_Deep.Persistence;

namespace Dive_Deep.Controllers
{
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(IBookingService bookingService, IRoomRepository roomRepository, UserManager<ApplicationUser> userManager)
        {
            _bookingService = bookingService;
            _productRepository = roomRepository;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var bookings = _bookingService.GetAll().Where(b => b.UserId == userId).ToList();
            return View(bookings);
        }

        public IActionResult Add(int? id)
        {
            ViewBag.Action = "add";

            var bookingVM = new BookingViewModel
            {
                Rooms = _productRepository.GetAll()
            };

            var date = DateTime.Now;
            bookingVM.Booking.StartTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            bookingVM.Booking.EndTime = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, date.Minute, 0);

            if (id != null) bookingVM.Booking.RoomId = id.Value;

            return View(bookingVM);
        }

        [HttpPost]
        public IActionResult Add(BookingViewModel bookingVM)
        {
            bookingVM.Booking.UserId = _userManager.GetUserId(User);
            ModelState.Remove("Booking.UserId");

            if (!ModelState.IsValid)
            {

                bookingVM.Rooms = _productRepository.GetAll();
                ViewBag.Action = "add";

                return View(bookingVM);
            }

            var result = _bookingService.Add(bookingVM.Booking);
            if (!result.IsSuccessful)
            {
                ModelState.AddModelError(result.Key, result.ErrorMessage);
                bookingVM.Rooms = _productRepository.GetAll();
                ViewBag.Action = "add";

                return View(bookingVM);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? id)
        {
            var booking = _bookingService.GetById(id ?? 0);
            if (booking == null)
            {
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);
            if (booking.UserId != userId)
            {
                return RedirectToAction("Index");
            }

            BookingViewModel bookingVM = new BookingViewModel
            {
                Booking = booking,
                Rooms = _productRepository.GetAll()
            };

            ViewBag.Action = "edit";

            return View(bookingVM);
        }

        [HttpPost]
        public IActionResult Edit(BookingViewModel bookingVM)
        {
            var existing = _bookingService.GetById(bookingVM.Booking.BookingId);
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
                bookingVM.Rooms = _productRepository.GetAll();

                ViewBag.Action = "edit";

                return View(bookingVM);
            }

            var result = _bookingService.Update(bookingVM.Booking);
            if (!result.IsSuccessful)
            {
                ModelState.AddModelError(result.Key, result.ErrorMessage);
                bookingVM.Rooms = _productRepository.GetAll();
                ViewBag.Action = "edit";

                return View(bookingVM);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var booking = _bookingService.GetById(id);
            if (booking == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (booking.UserId != userId)
            {
                return Forbid();
            }

            _bookingService.Delete(id);

            return RedirectToAction("Index");
        }
    }
}
