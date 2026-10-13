using Dive_Deep.Models.Validations;
using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.DTOs;

// ---------- Response ----------
public class BookingDto
{
    public int BookingId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public List<BookingItemDto> Items { get; set; } = new();
}

public class BookingItemDto
{
    public int BookingItemId { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string OptionLabel { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal DailyRate { get; set; }
}
// ---------- Request ----------
public class CreateBookingRequest
{
    [Required]
    [MinLength(1)]
    public List<CreateBookingItemRequest> Items { get; set; } = new();
}

public class CreateBookingItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductVariantId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    [FutureDate]
    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }
}