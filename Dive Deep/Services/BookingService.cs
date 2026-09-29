using Dive_Deep.Models;
using Dive_Deep.Persistence;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;

    public BookingService(IBookingRepository bookings) => _bookings = bookings;

    public Task<IReadOnlyList<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default) =>
        _bookings.GetAllBookingsAsync(cancellationToken);

    public Task<IReadOnlyList<Booking>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        _bookings.GetForUserAsync(userId, cancellationToken);

    public Task<Booking?> GetForUserAsync(
        int bookingId,
        string? userId,
        CancellationToken cancellationToken = default) =>
        _bookings.GetForUserAsync(bookingId, userId, cancellationToken);

    public async Task<BookingOperationResult> CreateAsync(
        string userId,
        IReadOnlyCollection<BookingLineRequest> lines,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BookingOperationResult.Failure("UserRequired", "Du skal være logget ind for at oprette en booking.");
        }

        if (lines.Count == 0)
        {
            return BookingOperationResult.Failure("LinesRequired", "Vælg mindst ét produkt til bookingen.");
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var line in lines)
        {
            var validation = Validate(line, now);
            if (validation is not null)
            {
                return validation;
            }
        }

        var booking = new Booking
        {
            UserId = userId,
            Status = BookingStatus.Confirmed,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        foreach (var line in lines)
        {
            booking.Items.Add(new BookingItem
            {
                Booking = booking,
                ProductVariantId = line.ProductVariantId,
                Quantity = line.Quantity,
                StartTime = line.StartTime.ToUniversalTime(),
                EndTime = line.EndTime.ToUniversalTime()
            });
        }

        var outcome = await _bookings.CreateWithAllocationsAsync(booking, cancellationToken);
        return outcome switch
        {
            BookingReservationOutcome.Reserved => BookingOperationResult.Success(booking.BookingId),
            BookingReservationOutcome.ProductVariantNotFound => BookingOperationResult.Failure(
                "ProductNotFound", "Det valgte udstyr findes ikke eller kan ikke lejes."),
            BookingReservationOutcome.Unavailable => BookingOperationResult.Failure(
                "Unavailable", "Der er ikke nok ledige enheder i den valgte periode."),
            _ => BookingOperationResult.Failure("BookingFailed", "Bookingen kunne ikke gennemføres. Prøv igen.")
        };
    }

    public async Task<BookingOperationResult> UpdateSingleLineAsync(
        int bookingId,
        int bookingItemId,
        string? userId,
        bool isAdmin,
        BookingLineRequest line,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(line, DateTimeOffset.UtcNow);
        if (validation is not null)
        {
            return validation;
        }

        var normalizedLine = line with
        {
            StartTime = line.StartTime.ToUniversalTime(),
            EndTime = line.EndTime.ToUniversalTime()
        };

        var outcome = await _bookings.UpdateSingleLineAsync(
            bookingId, bookingItemId, userId, isAdmin, normalizedLine, cancellationToken);

        return outcome switch
        {
            BookingReservationOutcome.Reserved => BookingOperationResult.Success(bookingId),
            BookingReservationOutcome.ProductVariantNotFound => BookingOperationResult.Failure(
                "ProductNotFound", "Det valgte udstyr findes ikke eller kan ikke lejes."),
            BookingReservationOutcome.Unavailable => BookingOperationResult.Failure(
                "Unavailable", "Der er ikke nok ledige enheder i den valgte periode."),
            BookingReservationOutcome.ConcurrencyConflict => BookingOperationResult.Failure(
                "ConcurrencyConflict", "Bookingen blev ændret samtidigt. Genindlæs siden og prøv igen."),
            _ => BookingOperationResult.Failure("BookingNotFound", "Bookingen blev ikke fundet.")
        };
    }

    public async Task<BookingOperationResult> CancelAsync(
        int bookingId,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var outcome = await _bookings.CancelAsync(bookingId, userId, isAdmin, cancellationToken);
        return outcome switch
        {
            BookingReservationOutcome.Reserved => BookingOperationResult.Success(bookingId),
            BookingReservationOutcome.ConcurrencyConflict => BookingOperationResult.Failure(
                "ConcurrencyConflict", "Bookingen blev ændret samtidigt. Genindlæs siden og prøv igen."),
            _ => BookingOperationResult.Failure("BookingNotFound", "Bookingen blev ikke fundet.")
        };
    }

    private static BookingOperationResult? Validate(BookingLineRequest line, DateTimeOffset now)
    {
        if (line.ProductVariantId <= 0)
        {
            return BookingOperationResult.Failure("ProductRequired", "Vælg venligst et produkt.");
        }

        if (line.Quantity < 1)
        {
            return BookingOperationResult.Failure("QuantityRequired", "Antallet skal være mindst 1.");
        }

        if (line.StartTime < now)
        {
            return BookingOperationResult.Failure("StartInPast", "Starttidspunktet må ikke ligge i fortiden.");
        }

        if (line.EndTime <= line.StartTime)
        {
            return BookingOperationResult.Failure("InvalidRange", "Sluttidspunktet skal ligge efter starttidspunktet.");
        }

        return null;
    }
}
