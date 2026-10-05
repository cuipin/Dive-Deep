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

    Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetManageableProductsAsync(CancellationToken cancellationToken = default);

    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<bool> CategoryNameExistsAsync(string categoryName, CancellationToken cancellationToken = default);

    Task<bool> ProductExistsAsync(
        int? categoryId,
        string? categoryName,
        string brand,
        string model,
        CancellationToken cancellationToken = default);

    Task<Product?> AddProductAsAdminAsync(Product product, CancellationToken cancellationToken = default);

    Task<ProductDeletionOutcome> DeleteProductAsAdminAsync(
        int productId,
        CancellationToken cancellationToken = default);
}
