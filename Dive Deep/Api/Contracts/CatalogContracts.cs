namespace Dive_Deep.Api.Contracts;

public sealed record ProductResponse(
    int ProductId,
    string Category,
    string Brand,
    string Model,
    string? ImagePath,
    IReadOnlyList<ProductVariantResponse> Variants);

public sealed record ProductVariantResponse(
    int ProductVariantId,
    string OptionLabel,
    string? Size,
    string? Gender,
    string? EquipmentType,
    decimal? ThicknessMm,
    decimal? VolumeLiters,
    decimal DailyRate,
    int RegisteredStock);
