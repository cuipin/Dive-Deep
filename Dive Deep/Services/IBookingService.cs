using Dive_Deep.Models;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Services;

public interface IBookingService
{
    Task<IReadOnlyList<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<Booking?> GetForUserAsync(int bookingId, string? userId, CancellationToken cancellationToken = default);

    Task<BookingOperationResult> CreateAsync(
        string userId,
        IReadOnlyCollection<BookingLineRequest> lines,
        CancellationToken cancellationToken = default);

    Task<BookingOperationResult> UpdateSingleLineAsync(
        int bookingId,
        int bookingItemId,
        string? userId,
        bool isAdmin,
        BookingLineRequest line,
        CancellationToken cancellationToken = default);

    Task<BookingOperationResult> CancelAsync(
        int bookingId,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);
}
