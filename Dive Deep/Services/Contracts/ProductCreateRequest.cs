namespace Dive_Deep.Services.Contracts;

/// <summary>Application-level input for creating a catalog product and its opening inventory.</summary>
public sealed class ProductCreateRequest
{
    public int? CategoryId { get; init; }
    public string? NewCategoryName { get; init; }
    public required string Brand { get; init; }
    public required string Model { get; init; }
    public required string ImageFileName { get; init; }
    public required IReadOnlyList<ProductVariantCreateRequest> Variants { get; init; }
}

public sealed class ProductVariantCreateRequest
{
    public required string OptionLabel { get; init; }
    public string? Size { get; init; }
    public string? Gender { get; init; }
    public string? EquipmentType { get; init; }
    public decimal? ThicknessMm { get; init; }
    public decimal? VolumeLiters { get; init; }
    public string? FirstStage { get; init; }
    public string? SecondStage { get; init; }
    public string? Octopus { get; init; }
    public decimal DailyRate { get; init; }
    public int InitialUnitCount { get; init; }
}
