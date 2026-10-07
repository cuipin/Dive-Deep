using Dive_Deep.Data;
using Dive_Deep.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Dive_Deep.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly Dive_DeepContext _DiveDeepContext;

    public ProductRepository(Dive_DeepContext Database) => _DiveDeepContext = Database;

    public async Task<Product?> AddProductAsAdminAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        _DiveDeepContext.Products.Add(product);
        try
        {
            await _DiveDeepContext.SaveChangesAsync(cancellationToken);
            return product;
        }
        catch (DbUpdateException exception) when (
            exception.GetBaseException() is SqlException sqlException
            && sqlException.Number is 2601 or 2627)
        {
            // The unique indexes remain the final protection against two simultaneous admin submissions.
            _DiveDeepContext.ChangeTracker.Clear();
            return null;
        }
    }

    public async Task<IReadOnlyList<ProductVariant>> GetProductVariantsAsync(
        int productId,
        CancellationToken cancellationToken = default) =>
        await _DiveDeepContext.ProductVariants
            .AsNoTracking()
            .Where(variant => variant.ProductId == productId)
            .Include(variant => variant.EquipmentUnits)
            .ToListAsync(cancellationToken);    

    public async Task<Product?> GetProductByIdAsync(
        int productId,
        CancellationToken cancellationToken = default) =>
        await _DiveDeepContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Variants.Where(variant => variant.IsActive))
                .ThenInclude(variant => variant.EquipmentUnits.Where(unit => unit.Status == EquipmentUnitStatus.Active))
            .SingleOrDefaultAsync(product => product.ProductId == productId, cancellationToken);

    public async Task<bool> UpdateProductAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        _DiveDeepContext.Products.Update(product);
        try
        {
            await _DiveDeepContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.GetBaseException() is SqlException sqlException
            && sqlException.Number is 2601 or 2627)
        {
            // The unique indexes remain the final protection against two simultaneous admin submissions.
            _DiveDeepContext.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await _DiveDeepContext.Products
            .Include(candidate => candidate.Variants)
                .ThenInclude(variant => variant.EquipmentUnits)
            .SingleOrDefaultAsync(candidate => candidate.ProductId == productId, cancellationToken);
        if (product is null)
        {
            return;
        }
        _DiveDeepContext.EquipmentUnits.RemoveRange(
            product.Variants.SelectMany(variant => variant.EquipmentUnits));
        _DiveDeepContext.ProductVariants.RemoveRange(product.Variants);
        _DiveDeepContext.Products.Remove(product);
        await _DiveDeepContext.SaveChangesAsync(cancellationToken);
    }

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
                .ThenInclude(variant => variant.EquipmentUnits.Where(unit => unit.Status == EquipmentUnitStatus.Active))
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
                .ThenInclude(variant => variant.EquipmentUnits.Where(unit => unit.Status == EquipmentUnitStatus.Active))
            .OrderBy(product => product.Brand)
            .ThenBy(product => product.Model)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductVariant>> GetBookingChoicesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _DiveDeepContext.ProductVariants
            .AsNoTracking()
            .Where(variant => variant.IsActive
                && variant.Product.IsActive
                && variant.EquipmentUnits.Any(unit => unit.Status == EquipmentUnitStatus.Active))
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
                && variant.Product.IsActive
                && variant.EquipmentUnits.Any(unit => unit.Status == EquipmentUnitStatus.Active))
            .Include(variant => variant.Product)
                .ThenInclude(product => product.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync(
        CancellationToken cancellationToken = default) =>
        await _DiveDeepContext.ProductCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetManageableProductsAsync(
        CancellationToken cancellationToken = default) =>
        await _DiveDeepContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Variants)
                .ThenInclude(variant => variant.EquipmentUnits)
            .OrderBy(product => product.Category.Name)
            .ThenBy(product => product.Brand)
            .ThenBy(product => product.Model)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default) =>
        await _DiveDeepContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Variants)
                .ThenInclude(variant => variant.EquipmentUnits)
            .OrderBy(product => product.Category.Name)
            .ThenBy(product => product.Brand)
            .ThenBy(product => product.Model)
            .ToListAsync(cancellationToken);

    public Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default) =>
        _DiveDeepContext.ProductCategories.AnyAsync(category =>
            category.ProductCategoryId == categoryId,
            cancellationToken);

    public Task<bool> CategoryNameExistsAsync(string categoryName, CancellationToken cancellationToken = default) =>
        _DiveDeepContext.ProductCategories.AnyAsync(category => category.Name == categoryName, cancellationToken);

    public Task<bool> ProductExistsAsync(
        int? categoryId,
        string? categoryName,
        string brand,
        string model,
        CancellationToken cancellationToken = default) =>
        _DiveDeepContext.Products.AnyAsync(product =>
            (categoryId.HasValue
                ? product.ProductCategoryId == categoryId.Value
                : product.Category.Name == categoryName)
            && product.Brand == brand
            && product.Model == model,
            cancellationToken);

    public async Task<ProductDeletionOutcome> DeleteProductAsAdminAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _DiveDeepContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var product = await _DiveDeepContext.Products
            .Include(candidate => candidate.Variants)
                .ThenInclude(variant => variant.EquipmentUnits)
            .SingleOrDefaultAsync(candidate => candidate.ProductId == productId, cancellationToken);

        if (product is null)
        {
            return new ProductDeletionOutcome(ProductDeletionStatus.NotFound);
        }

        var variantIds = product.Variants.Select(variant => variant.ProductVariantId).ToArray();
        var isReferencedByBookings = await _DiveDeepContext.BookingItems
            .AnyAsync(item => variantIds.Contains(item.ProductVariantId), cancellationToken);
        var isReferencedByCarts = await _DiveDeepContext.CartItems
            .AnyAsync(item => variantIds.Contains(item.ProductVariantId), cancellationToken);

        if (isReferencedByBookings || isReferencedByCarts)
        {
            product.IsActive = false;
            foreach (var variant in product.Variants)
            {
                variant.IsActive = false;
                foreach (var unit in variant.EquipmentUnits)
                {
                    if (unit.Status == EquipmentUnitStatus.Active)
                    {
                        unit.Status = EquipmentUnitStatus.Retired;
                    }
                }
            }

            await _DiveDeepContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new ProductDeletionOutcome(ProductDeletionStatus.Archived);
        }

        var unusedImageFileName = product.ImageFileName;
        if (!string.IsNullOrWhiteSpace(unusedImageFileName))
        {
            var imageIsShared = await _DiveDeepContext.Products.AnyAsync(
                candidate => candidate.ProductId != product.ProductId
                    && candidate.ImageFileName == unusedImageFileName,
                cancellationToken);
            if (imageIsShared)
            {
                unusedImageFileName = null;
            }
        }

        _DiveDeepContext.EquipmentUnits.RemoveRange(
            product.Variants.SelectMany(variant => variant.EquipmentUnits));
        _DiveDeepContext.ProductVariants.RemoveRange(product.Variants);
        _DiveDeepContext.Products.Remove(product);

        await _DiveDeepContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new ProductDeletionOutcome(ProductDeletionStatus.Deleted, unusedImageFileName);
    }
}
