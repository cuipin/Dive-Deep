using System.Security.Claims;
using Dive_Deep.Api;
using Dive_Deep.Api.Contracts;
using Dive_Deep.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Api.Controllers;

[ApiController]
[Authorize(Policy = ApiPolicies.MobileApi)]
[Route("api/v1/cart")]
[Produces("application/json")]
public sealed class CartApiController(ICartService carts) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CartResponse>> Get(CancellationToken cancellationToken) =>
        Ok((await carts.GetForUserAsync(CurrentUserId, cancellationToken)).ToApi());

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var result = await carts.AddItemAsync(CurrentUserId, request.ProductVariantId, request.Quantity, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.ErrorMessage });
    }

    [HttpPost("packages")]
    public async Task<IActionResult> AddPackage(AddCartPackageRequest request, CancellationToken cancellationToken)
    {
        var result = await carts.AddPackageAsync(
            CurrentUserId,
            request.PackageType,
            request.ProductVariantIds,
            request.Quantity,
            cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.ErrorMessage });
    }

    [HttpPut("items/{cartItemId:int}/rental-period")]
    public async Task<IActionResult> UpdateRentalPeriod(
        int cartItemId,
        UpdateCartRentalPeriodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await carts.UpdateRentalPeriodAsync(
            CurrentUserId,
            cartItemId,
            request.StartTime,
            request.EndTime,
            cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.ErrorMessage });
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken cancellationToken)
    {
        var result = await carts.RemoveItemAsync(CurrentUserId, cartItemId, cancellationToken);
        return result.Succeeded ? NoContent() : NotFound(new { message = result.ErrorMessage });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var result = await carts.CheckoutAsync(CurrentUserId, cancellationToken);
        return result.Succeeded
            ? Ok(new { bookingId = result.BookingId, message = "Din leje er bekræftet." })
            : ToBookingFailure(result.ErrorCode, result.ErrorMessage);
    }

    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new InvalidOperationException("JWT'en mangler brugerens id.");

    private IActionResult ToBookingFailure(string? errorCode, string? errorMessage) => errorCode switch
    {
        "Unavailable" or "ConcurrencyConflict" => Conflict(new { errorCode, message = errorMessage }),
        _ => BadRequest(new { errorCode, message = errorMessage })
    };
}
