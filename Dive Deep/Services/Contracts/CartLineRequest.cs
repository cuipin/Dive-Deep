namespace Dive_Deep.Services.Contracts;

public sealed record CartLineRequest(
    int ProductVariantId,
    int Quantity);
