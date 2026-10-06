using Dive_Deep.Models;

namespace Dive_Deep.ViewModels;

public class ProductCatalogViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = Array.Empty<Product>();
    public IReadOnlyList<ProductVariant> PackageVariants { get; init; } = Array.Empty<ProductVariant>();
    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();
    public string? Search { get; init; }
    public string? Category { get; init; }
    public decimal? MinimumDailyRate { get; init; }
    public decimal? MaximumDailyRate { get; init; }
    public string? FilterError { get; init; }

    public string Heading => string.IsNullOrWhiteSpace(Category) ? "Alt dykkerudstyr" : Category;
}
