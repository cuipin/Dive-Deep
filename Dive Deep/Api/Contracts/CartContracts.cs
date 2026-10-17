using System.ComponentModel.DataAnnotations;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Api.Contracts;

public sealed class AddCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductVariantId { get; init; }

    [Range(1, 100)]
    public int Quantity { get; init; }
}

public sealed class AddCartPackageRequest
{
    public RentalPackageType PackageType { get; init; }

    [MinLength(1)]
    public int[] ProductVariantIds { get; init; } = [];

    [Range(1, 100)]
    public int Quantity { get; init; }
}

public sealed class UpdateCartRentalPeriodRequest
{
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
}

public sealed record CartResponse(int CartId, IReadOnlyList<CartItemResponse> Items);

public sealed record CartItemResponse(
    int CartItemId,
    int Quantity,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    ProductVariantResponse ProductVariant);
