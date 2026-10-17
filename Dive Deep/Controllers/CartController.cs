using Dive_Deep.Data;
using Dive_Deep.Services;
using Dive_Deep.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _carts;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(
        ICartService carts,
        UserManager<ApplicationUser> userManager)
    {
        _carts = carts;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cart = await _carts.GetForUserAsync(CurrentUserId, cancellationToken);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(
        int productVariantId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var result = await _carts.AddItemAsync(
            CurrentUserId,
            productVariantId,
            quantity,
            cancellationToken);

        SetCartMessage(result, "Produktet er lagt i kurven.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPackage(
        RentalPackageType packageType,
        int[] productVariantIds,
        int quantity,
        CancellationToken cancellationToken)
    {
        var result = await _carts.AddPackageAsync(
            CurrentUserId,
            packageType,
            productVariantIds,
            quantity,
            cancellationToken);

        SetCartMessage(result, "Sættet er lagt i kurven.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRentalPeriod(
        int id,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken)
    {
        if (!TryConvertTimes(startTime, endTime, out var start, out var end))
        {
            return RedirectToAction(nameof(Index));
        }

        var result = await _carts.UpdateRentalPeriodAsync(
            CurrentUserId,
            id,
            start,
            end,
            cancellationToken);

        SetCartMessage(result, "Lejeperioden er gemt.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await _carts.RemoveItemAsync(CurrentUserId, id, cancellationToken);
        SetCartMessage(result, "Produktet er fjernet fra kurven.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var result = await _carts.CheckoutAsync(CurrentUserId, cancellationToken);
        if (!result.Succeeded)
        {
            TempData["CartError"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        TempData["CartSuccess"] = $"Din leje er bekræftet. Bookingnummer: {result.BookingId}.";
        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId => _userManager.GetUserId(User)!;

    private void SetCartMessage(CartOperationResult result, string successMessage)
    {
        TempData[result.Succeeded ? "CartSuccess" : "CartError"] =
            result.Succeeded ? successMessage : result.ErrorMessage;
    }

    private bool TryConvertTimes(
        DateTime startTime,
        DateTime endTime,
        out DateTimeOffset start,
        out DateTimeOffset end)
    {
        try
        {
            start = DanishDateTime.ToUtc(startTime);
            end = DanishDateTime.ToUtc(endTime);
            return true;
        }
        catch (ArgumentException exception)
        {
            start = default;
            end = default;
            TempData["CartError"] = exception.Message;
            return false;
        }
    }
}
