using Dive_Deep.Models;
using Dive_Deep.Persistence;

namespace Dive_Deep.Services;

public class ProductService : IProductService
{
    private static readonly string[] FeaturedModelOrder =
    [
        "BCD Hydros Pro",
        "Definition",
        "Regulatorsæt",
        "Jet Fin"
    ];

    private readonly IProductRepository _products;

    public ProductService(IProductRepository products) => _products = products;

    public Task<IReadOnlyList<Product>> SearchProductsAsync(
        string? search,
        string? categoryName,
        decimal? minimumDailyRate,
        decimal? maximumDailyRate,
        CancellationToken cancellationToken = default) =>
        _products.SearchProductsAsync(search, categoryName, minimumDailyRate, maximumDailyRate, cancellationToken);

    public Task<IReadOnlyList<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default) =>
        _products.GetCategoryNamesAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return Array.Empty<Product>();
        }

        var catalog = await _products.SearchProductsAsync(null, null, null, null, cancellationToken);
        var featured = FeaturedModelOrder
            .Select(name => catalog.FirstOrDefault(product => product.Model == name))
            .Where(product => product is not null)
            .Cast<Product>()
            .ToList();

        featured.AddRange(catalog.Where(product => !featured.Contains(product)));
        return featured.Take(count).ToArray();
    }

    public Task<IReadOnlyList<Product>> GetByCategoryNameAsync(
        string categoryName,
        CancellationToken cancellationToken = default) =>
        _products.GetByCategoryNameAsync(categoryName, cancellationToken);

    public Task<IReadOnlyList<ProductVariant>> GetBookingChoicesAsync(
        CancellationToken cancellationToken = default) =>
        _products.GetBookingChoicesAsync(cancellationToken);
}
