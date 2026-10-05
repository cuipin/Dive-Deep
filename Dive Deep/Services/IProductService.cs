using Dive_Deep.Models;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> SearchProductsAsync(
        string? search,
        string? categoryName,
        decimal? minimumDailyRate,
        decimal? maximumDailyRate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetByCategoryNameAsync(
        string categoryName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductVariant>> GetBookingChoicesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetManageableProductsAsync(CancellationToken cancellationToken = default);

    Task<ProductCreateResult> CreateProductAsync(
        ProductCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<ProductDeleteResult> DeleteProductAsAdminAsync(
        int productId,
        CancellationToken cancellationToken = default);
}
