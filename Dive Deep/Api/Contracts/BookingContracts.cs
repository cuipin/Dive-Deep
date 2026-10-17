using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.Api.Contracts;

public sealed class CreateBookingRequest
{
    [MinLength(1)]
    public List<BookingLineInput> Lines { get; init; } = [];
}

public class BookingLineInput
{
    [Range(1, int.MaxValue)]
    public int ProductVariantId { get; init; }

    [Range(1, 100)]
    public int Quantity { get; init; }

    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
}

public sealed class UpdateBookingItemRequest : BookingLineInput
{
    [Required]
    public string RowVersion { get; init; } = string.Empty;
}

public sealed record BookingResponse(
    int BookingId,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string RowVersion,
    IReadOnlyList<BookingItemResponse> Items);

public sealed record BookingItemResponse(
    int BookingItemId,
    int ProductVariantId,
    string Category,
    string Brand,
    string Model,
    string OptionLabel,
    int Quantity,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal DailyRateAtBooking);
