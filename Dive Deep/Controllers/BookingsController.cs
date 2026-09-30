using Dive_Deep.Data;
using Dive_Deep.Models;
using Dive_Deep.Services;
using Dive_Deep.Services.Contracts;
using Dive_Deep.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly IBookingService _bookings;
    private readonly IProductService _products;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingsController(
        IBookingService bookings,
        IProductService products,
        UserManager<ApplicationUser> userManager)
    {
        _bookings = bookings;
        _products = products;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = _userManager.GetUserId(User)!;

        IReadOnlyList<Booking> bookings;

        if (isAdmin)
        {
            bookings = await _bookings.GetAllBookingsAsync(cancellationToken);
        }
        else
        {
            bookings = await _bookings.GetForUserAsync(userId, cancellationToken);
        }

        foreach (var booking in bookings)
        {
            foreach (var item in booking.Items)
            {
                item.StartTime = TimeZoneInfo.ConvertTime(item.StartTime, DanishDateTime.TimeZone);
                item.EndTime = TimeZoneInfo.ConvertTime(item.EndTime, DanishDateTime.TimeZone);
            }
        }

        ViewBag.IsAdmin = isAdmin;
        return View(bookings.ToList());
    }

    public async Task<IActionResult> Add(int? id, CancellationToken cancellationToken)
    {
        ViewBag.Action = "add";
        var start = DanishDateTime.ToLocal(DateTimeOffset.UtcNow);
        start = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0).AddMinutes(2);

        var model = new BookingViewModel
        {
            Booking = new BookingFormInput
            {
                ProductId = id ?? 0,
                StartTime = start,
                EndTime = start.AddHours(1)
            },
            Products = await GetProductChoicesAsync(cancellationToken)
        };

        return View("BookingView", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        BookingViewModel model,
        CancellationToken cancellationToken)
    {
        ViewBag.Action = "add";
        if (!ModelState.IsValid)
        {
            model.Products = await GetProductChoicesAsync(cancellationToken);
            return View("BookingView", model);
        }

        if (!TryConvertTimes(model.Booking, out var start, out var end))
        {
            model.Products = await GetProductChoicesAsync(cancellationToken);
            return View("BookingView", model);
        }

        var userId = _userManager.GetUserId(User)!;
        var result = await _bookings.CreateAsync(
            userId,
            [new BookingLineRequest(model.Booking.ProductId, model.Booking.Quantity, start, end)],
            cancellationToken);

        if (!result.Succeeded)
        {
            AddBookingError(result);
            model.Products = await GetProductChoicesAsync(cancellationToken);
            return View("BookingView", model);
        }

        TempData["BookingSuccess"] = "Bookingen blev oprettet.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(
        int? id,
        int? itemId,
        CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        var booking = await _bookings.GetForUserAsync(
            id ?? 0,
            isAdmin ? null : _userManager.GetUserId(User),
            cancellationToken);
        var item = itemId.HasValue
            ? booking?.Items.FirstOrDefault(candidate => candidate.BookingItemId == itemId.Value)
            : booking?.Items.OrderBy(candidate => candidate.BookingItemId).FirstOrDefault();

        if (booking is null || item is null || booking.Status != BookingStatus.Confirmed)
        {
            return NotFound();
        }

        ViewBag.Action = "edit";
        var model = new BookingViewModel
        {
            Booking = new BookingFormInput
            {
                BookingId = booking.BookingId,
                BookingItemId = item.BookingItemId,
                ProductId = item.ProductVariantId,
                Quantity = item.Quantity,
                RowVersion = booking.RowVersion,
                StartTime = DanishDateTime.ToLocal(item.StartTime),
                EndTime = DanishDateTime.ToLocal(item.EndTime)
            },
            Products = await GetProductChoicesAsync(cancellationToken),
            CustomerName = $"{booking.User?.FirstName} {booking.User?.LastName}".Trim(),
            CustomerEmail = booking.User?.Email
        };

        ViewBag.IsAdmin = isAdmin;
        return View("BookingView", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        BookingViewModel model,
        CancellationToken cancellationToken)
    {
        ViewBag.Action = "edit";
        ViewBag.IsAdmin = User.IsInRole("Admin");
        if (!ModelState.IsValid)
        {
            await PopulateEditDetailsAsync(model, cancellationToken);
            return View("BookingView", model);
        }

        if (!TryConvertTimes(model.Booking, out var start, out var end))
        {
            await PopulateEditDetailsAsync(model, cancellationToken);
            return View("BookingView", model);
        }

        var result = await _bookings.UpdateSingleLineAsync(
            model.Booking.BookingId,
            model.Booking.BookingItemId,
            _userManager.GetUserId(User)!,
            model.Booking.RowVersion,
            User.IsInRole("Admin"),
            new BookingLineRequest(model.Booking.ProductId, model.Booking.Quantity, start, end),
            cancellationToken);

        if (!result.Succeeded)
        {
            AddBookingError(result);
            await PopulateEditDetailsAsync(model, cancellationToken);
            return View("BookingView", model);
        }
        TempData["BookingSuccess"] = "Bookingens udstyr og lejeperiode blev opdateret.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _bookings.CancelAsync(
            id,
            _userManager.GetUserId(User)!,
            User.IsInRole("Admin"),
            cancellationToken);

        if (!result.Succeeded)
        {
            return NotFound();
        }

        TempData["BookingSuccess"] = "Bookingen blev annulleret.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<ProductChoiceViewModel>> GetProductChoicesAsync(
        CancellationToken cancellationToken)
    {
        var variants = await _products.GetBookingChoicesAsync(cancellationToken);
        return variants.Select(variant => new ProductChoiceViewModel
        {
            ProductId = variant.ProductVariantId,
            Category = variant.Product.Category.Name,
            Mærke = variant.Product.Brand,
            Model = variant.Product.Model,
            OptionLabel = variant.OptionLabel
        }).ToList();
    }

    private async Task PopulateEditDetailsAsync(
        BookingViewModel model,
        CancellationToken cancellationToken)
    {
        model.Products = await GetProductChoicesAsync(cancellationToken);
        var isAdmin = User.IsInRole("Admin");
        var booking = await _bookings.GetForUserAsync(
            model.Booking.BookingId,
            isAdmin ? null : _userManager.GetUserId(User),
            cancellationToken);

        if (isAdmin && booking?.User is not null)
        {
            model.CustomerName = $"{booking.User.FirstName} {booking.User.LastName}".Trim();
            model.CustomerEmail = booking.User.Email;
        }
    }

    private bool TryConvertTimes(BookingFormInput input, out DateTimeOffset start, out DateTimeOffset end)
    {
        try
        {
            start = DanishDateTime.ToUtc(input.StartTime);
            end = DanishDateTime.ToUtc(input.EndTime);
            return true;
        }
        catch (ArgumentException exception)
        {
            start = default;
            end = default;
            ModelState.AddModelError("Booking.StartTime", exception.Message);
            return false;
        }
    }

    private void AddBookingError(BookingOperationResult result)
    {
        var key = result.ErrorCode switch
        {
            "StartInPast" => "Booking.StartTime",
            "InvalidRange" => "Booking.EndTime",
            "ConcurrencyConflict" => string.Empty,
            _ => "Booking.ProductId"
        };

        ModelState.AddModelError(key, result.ErrorMessage ?? "Bookingen kunne ikke gennemføres.");
    }
}