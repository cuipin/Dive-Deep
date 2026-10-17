using Dive_Deep.Api;
using Dive_Deep.Api.Contracts;
using Dive_Deep.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Api.Controllers;

[ApiController]
[Authorize(Policy = ApiPolicies.MobileApi, Roles = "Admin")]
[Route("api/v1/admin/bookings")]
[Produces("application/json")]
public sealed class AdminBookingsApiController(IBookingService bookings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll(CancellationToken cancellationToken)
    {
        // Rollekravet ovenfor betyder, at kun en JWT med Admin-rollen kommer helt herind.
        var allBookings = await bookings.GetAllBookingsAsync(cancellationToken);
        return Ok(allBookings.Select(booking => booking.ToApi()).ToArray());
    }
}
