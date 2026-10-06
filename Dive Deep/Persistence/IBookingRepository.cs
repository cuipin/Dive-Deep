using Dive_Deep.Models;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Persistence;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetForUserBookingsAsync(
        string userId,
        bool history,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<Booking?> GetForUserAsync(int bookingId, string? userId, CancellationToken cancellationToken = default);

    Task<BookingReservationOutcome> CreateWithAllocationsAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<BookingReservationOutcome> UpdateSingleLineAsync(
        int bookingId,
        int bookingItemId,
        string? userId,
        byte[] expectedRowVersion,
        bool isAdmin,
        BookingLineRequest line,
        CancellationToken cancellationToken = default);

    Task<BookingReservationOutcome> CancelAsync(
        int bookingId,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
