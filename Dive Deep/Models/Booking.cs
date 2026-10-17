using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dive_Deep.Data;

namespace Dive_Deep.Models;

public enum BookingStatus
{
    Confirmed,
    Cancelled,
    Completed
}

public class Booking
{
    public int BookingId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();

    // Read-only adapters keep the existing list view usable until it is redesigned
    // to show every booking line.
    [NotMapped]
    public int ProductId => Items.OrderBy(item => item.BookingItemId).FirstOrDefault()?.ProductVariantId ?? 0;

    [NotMapped]
    public DateTimeOffset StartTime => Items.OrderBy(item => item.BookingItemId).FirstOrDefault()?.StartTime ?? default;

    [NotMapped]
    public DateTimeOffset EndTime => Items.OrderBy(item => item.BookingItemId).FirstOrDefault()?.EndTime ?? default;
}
