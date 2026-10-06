using Dive_Deep.Models;

namespace Dive_Deep.Persistence;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> SearchProductsAsync(
        string? search,
        string? categoryName,
        decimal? minimumDailyRate,
        decimal? maximumDailyRate,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByCategoryNameAsync(string categoryName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductVariant>> GetBookingChoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductVariant>> GetVariantsByIdsAsync(
        IReadOnlyCollection<int> productVariantIds,
        CancellationToken cancellationToken = default);
}
