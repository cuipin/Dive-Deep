using System.Security.Claims;
using Dive_Deep.DTOs;
using Dive_Deep.Services;
using Dive_Deep.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Dive_Deep.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ApiBookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public ApiBookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: api/bookings
        [HttpGet]
        public async Task<IActionResult> GetMine()
        {
            var bookings = await _bookingService.GetForUserAsync(UserId);
            return Ok(bookings.Select(b => b.ToDto()));
        }

        // GET: api/bookings/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var booking = await _bookingService.GetForUserAsync(id, UserId);
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking.ToDto());
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
        {
            var lines = request.Items
                .Select(i => new BookingLineRequest(i.ProductVariantId, i.Quantity, i.StartTime, i.EndTime))
                .ToList();

            var result = await _bookingService.CreateAsync(UserId, lines);
            if (!result.Succeeded)
            {
                return ToErrorResult(result);
            }

            var booking = await _bookingService.GetForUserAsync(result.BookingId!.Value, UserId);
            return CreatedAtAction(nameof(GetById), new { id = result.BookingId }, booking!.ToDto());
        }

        // POST: api/bookings/5/cancel
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel([FromRoute] int id)
        {
            var result = await _bookingService.CancelAsync(id, UserId, User.IsInRole("Admin"));
            return result.Succeeded ? NoContent() : ToErrorResult(result);
        }

        private IActionResult ToErrorResult(BookingOperationResult result)
        {
            var code = result.ErrorCode ?? string.Empty;
            var message = result.ErrorMessage ?? "Handlingen kunne ikke gennemføres.";

            if (code.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(message);
            }

            if (code.Contains("Availab", StringComparison.OrdinalIgnoreCase)
                || code.Contains("Conflict", StringComparison.OrdinalIgnoreCase)
                || code.Contains("Concurren", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(message);
            }

            return BadRequest(message);
        }
    }
}