using System.Security.Claims;
using Dive_Deep.Api;
using Dive_Deep.Api.Contracts;
using Dive_Deep.Services;
using Dive_Deep.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Api.Controllers;

[ApiController]
[Authorize(Policy = ApiPolicies.MobileApi)]
[Route("api/v1/bookings")]
[Produces("application/json")]
public sealed class BookingsApiController(IBookingService bookings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetBookings(
        bool history,
        CancellationToken cancellationToken)
    {
        var list = await bookings.GetForUserBookingsAsync(
            CurrentUserId,
            history,
            DateTimeOffset.UtcNow,
            cancellationToken);
        return Ok(list.Select(booking => booking.ToApi()).ToArray());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(line => new BookingLineRequest(
            line.ProductVariantId,
            line.Quantity,
            line.StartTime,
            line.EndTime)).ToArray();
        var result = await bookings.CreateAsync(CurrentUserId, lines, cancellationToken);

        return result.Succeeded
            ? CreatedAtAction(nameof(GetBookings), new { history = false }, new { bookingId = result.BookingId })
            : ToFailure(result);
    }

    [HttpPut("{bookingId:int}/items/{bookingItemId:int}")]
    public async Task<IActionResult> UpdateItem(
        int bookingId,
        int bookingItemId,
        UpdateBookingItemRequest request,
        CancellationToken cancellationToken)
    {
        byte[] rowVersion;
        try
        {
            rowVersion = Convert.FromBase64String(request.RowVersion);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "RowVersion skal være en gyldig Base64-værdi fra GET /api/v1/bookings." });
        }

        // RowVersion gør denne opdatering optimistisk låst: klienten skal sende den version, den læste.
        var result = await bookings.UpdateSingleLineAsync(
            bookingId,
            bookingItemId,
            CurrentUserId,
            rowVersion,
            isAdmin: false,
            new BookingLineRequest(request.ProductVariantId, request.Quantity, request.StartTime, request.EndTime),
            cancellationToken);
        return result.Succeeded ? NoContent() : ToFailure(result);
    }

    [HttpDelete("{bookingId:int}")]
    public async Task<IActionResult> Cancel(int bookingId, CancellationToken cancellationToken)
    {
        var result = await bookings.CancelAsync(bookingId, CurrentUserId, isAdmin: false, cancellationToken);
        return result.Succeeded ? NoContent() : NotFound(new { message = result.ErrorMessage });
    }

    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new InvalidOperationException("JWT'en mangler brugerens id.");

    private IActionResult ToFailure(BookingOperationResult result) => result.ErrorCode switch
    {
        "Unavailable" or "ConcurrencyConflict" => Conflict(new { result.ErrorCode, result.ErrorMessage }),
        "BookingNotFound" => NotFound(new { result.ErrorCode, result.ErrorMessage }),
        _ => BadRequest(new { result.ErrorCode, result.ErrorMessage })
    };
}
