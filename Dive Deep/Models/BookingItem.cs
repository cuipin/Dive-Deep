namespace Dive_Deep.Models;

public class BookingItem
{
    public int BookingItemId { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal DailyRateAtBooking { get; set; }
    public ICollection<BookingAllocation> Allocations { get; set; } = new List<BookingAllocation>();
}
