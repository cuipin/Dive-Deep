using Dive_Deep.Api.Contracts;
using Dive_Deep.Models;

namespace Dive_Deep.Api;

internal static class ApiMappings
{
    public static ProductResponse ToApi(this Product product) => new(
        product.ProductId,
        product.Category.Name,
        product.Brand,
        product.Model,
        string.IsNullOrWhiteSpace(product.ImageFileName) ? null : $"/lib/Images/{product.ImageFileName}",
        product.Variants
            .Where(variant => variant.IsActive)
            .OrderBy(variant => variant.OptionLabel)
            .Select(variant => variant.ToApi())
            .ToArray());

    public static ProductVariantResponse ToApi(this ProductVariant variant) => new(
        variant.ProductVariantId,
        variant.OptionLabel,
        variant.Size,
        variant.Gender,
        variant.EquipmentType,
        variant.ThicknessMm,
        variant.VolumeLiters,
        variant.DailyRate,
        variant.EquipmentUnits.Count(unit => unit.Status == EquipmentUnitStatus.Active));

    public static CartResponse ToApi(this Cart cart) => new(
        cart.CartId,
        cart.Items.OrderBy(item => item.CartItemId).Select(item => new CartItemResponse(
            item.CartItemId,
            item.Quantity,
            item.StartTime,
            item.EndTime,
            item.ProductVariant.ToApi())).ToArray());

    public static BookingResponse ToApi(this Booking booking) => new(
        booking.BookingId,
        booking.Status.ToString(),
        booking.CreatedAtUtc,
        booking.UpdatedAtUtc,
        Convert.ToBase64String(booking.RowVersion),
        booking.Items.OrderBy(item => item.BookingItemId).Select(item => new BookingItemResponse(
            item.BookingItemId,
            item.ProductVariantId,
            item.ProductVariant.Product.Category.Name,
            item.ProductVariant.Product.Brand,
            item.ProductVariant.Product.Model,
            item.ProductVariant.OptionLabel,
            item.Quantity,
            item.StartTime,
            item.EndTime,
            item.DailyRateAtBooking)).ToArray());
}
