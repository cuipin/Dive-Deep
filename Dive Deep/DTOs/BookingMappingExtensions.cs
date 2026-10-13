using Dive_Deep.Models;

namespace Dive_Deep.DTOs;

public static class BookingMappingExtensions
{
    public static BookingItemDto ToDto(this BookingItem item) => new()
    {
        BookingItemId = item.BookingItemId,
        ProductVariantId = item.ProductVariantId,
        ProductName = $"{item.ProductVariant.Product.Brand} {item.ProductVariant.Product.Model}",
        OptionLabel = item.ProductVariant.OptionLabel,
        Quantity = item.Quantity,
        DailyRate = item.DailyRateAtBooking
    };

    public static BookingDto ToDto(this Booking booking) => new()
    {
        BookingId = booking.BookingId,
        Status = booking.Status.ToString(),
        TotalPrice = booking.Items.Sum(i => i.DailyRateAtBooking * i.Quantity),
        Items = booking.Items.Select(i => i.ToDto()).ToList()
    };
}