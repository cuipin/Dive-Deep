using Dive_Deep.Models;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Services;

public interface IBookingService
{
    Task<IReadOnlyList<Booking>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Booking?> GetForUserAsync(int bookingId, string userId, CancellationToken cancellationToken = default);
    Task<BookingOperationResult> CreateAsync(
        string userId,
        IReadOnlyCollection<BookingLineRequest> lines,
        CancellationToken cancellationToken = default);
    Task<BookingOperationResult> UpdateSingleLineAsync(
        int bookingId,
        string userId,
        BookingLineRequest line,
        CancellationToken cancellationToken = default);
    Task<BookingOperationResult> CancelAsync(
        int bookingId,
        string userId,
        CancellationToken cancellationToken = default);
}
