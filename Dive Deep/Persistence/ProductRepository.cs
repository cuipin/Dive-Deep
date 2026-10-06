using Dive_Deep.Data;
using Dive_Deep.Models;
using Microsoft.EntityFrameworkCore;

namespace Dive_Deep.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly Dive_DeepContext _DiveDeepContext;

    public ProductRepository(Dive_DeepContext Database) => _DiveDeepContext = Database;

    public async Task<IReadOnlyList<Product>> SearchProductsAsync(
        string? search,
        string? categoryName,
        decimal? minimumDailyRate,
        decimal? maximumDailyRate,
        CancellationToken cancellationToken = default)
    {
        var products = _DiveDeepContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            products = products.Where(product => product.Category.Name == categoryName);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            products = products.Where(product =>
                product.Brand.Contains(searchTerm)
                || product.Model.Contains(searchTerm)
                || product.Category.Name.Contains(searchTerm));
        }

        if (minimumDailyRate.HasValue || maximumDailyRate.HasValue)
        {
            products = products.Where(product => product.Variants.Any(variant =>
                variant.IsActive
                && (!minimumDailyRate.HasValue || variant.DailyRate >= minimumDailyRate.Value)
                && (!maximumDailyRate.HasValue || variant.DailyRate <= maximumDailyRate.Value)));
        }

        return await products
            .Include(product => product.Category)
            .Include(product => product.Variants.Where(variant => variant.IsActive))
            .OrderBy(product => product.Category.Name)
            .ThenBy(product => product.Brand)
            .ThenBy(product => product.Model)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetCategoryNamesAsync(
        CancellationToken cancellationToken = default) =>
        await _DiveDeepContext.ProductCategories
            .AsNoTracking()
            .Where(category => category.Products.Any(product => product.IsActive))
            .OrderBy(category => category.Name)
            .Select(category => category.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByCategoryNameAsync(
        string categoryName,
        CancellationToken cancellationToken = default)
    {
        return await _DiveDeepContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.Category.Name == categoryName)
            .Include(product => product.Category)
            .Include(product => product.Variants.Where(variant => variant.IsActive))
            .OrderBy(product => product.Brand)
            .ThenBy(product => product.Model)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductVariant>> GetBookingChoicesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _DiveDeepContext.ProductVariants
            .AsNoTracking()
            .Where(variant => variant.IsActive && variant.Product.IsActive)
            .Include(variant => variant.Product)
                .ThenInclude(product => product.Category)
            .OrderBy(variant => variant.Product.Category.Name)
            .ThenBy(variant => variant.Product.Brand)
            .ThenBy(variant => variant.Product.Model)
            .ThenBy(variant => variant.OptionLabel)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductVariant>> GetVariantsByIdsAsync(
        IReadOnlyCollection<int> productVariantIds,
        CancellationToken cancellationToken = default)
    {
        var ids = productVariantIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return Array.Empty<ProductVariant>();
        }

        return await _DiveDeepContext.ProductVariants
            .AsNoTracking()
            .Where(variant => ids.Contains(variant.ProductVariantId)
                && variant.IsActive
                && variant.Product.IsActive)
            .Include(variant => variant.Product)
                .ThenInclude(product => product.Category)
            .ToListAsync(cancellationToken);
    }
}