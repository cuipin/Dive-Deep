using Dive_Deep.Models;
using Dive_Deep.Persistence;
using Dive_Deep.Services.Contracts;

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

    public Task<IReadOnlyList<ProductCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default) =>
        _products.GetAllCategoriesAsync(cancellationToken);

    public Task<IReadOnlyList<Product>> GetManageableProductsAsync(CancellationToken cancellationToken = default) =>
        _products.GetManageableProductsAsync(cancellationToken);

    public async Task<ProductCreateResult> CreateProductAsync(
        ProductCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var brand = request.Brand.Trim();
        var model = request.Model.Trim();
        var categoryName = request.NewCategoryName?.Trim();
        var hasNewCategoryName = !string.IsNullOrWhiteSpace(categoryName);

        if (string.IsNullOrWhiteSpace(brand)
            || string.IsNullOrWhiteSpace(model)
            || string.IsNullOrWhiteSpace(request.ImageFileName)
            || request.Variants.Count == 0
            || request.CategoryId.HasValue == hasNewCategoryName
            || request.Variants.Any(variant =>
                string.IsNullOrWhiteSpace(variant.OptionLabel)
                || variant.DailyRate <= 0
                || variant.InitialUnitCount is < 0 or > 5000
                || variant.ThicknessMm < 0
                || variant.VolumeLiters < 0)
            || request.Variants
                .Select(variant => variant.OptionLabel.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != request.Variants.Count)
        {
            return new ProductCreateResult(ProductCreateStatus.InvalidRequest);
        }

        if (request.CategoryId is int categoryId)
        {
            if (!await _products.CategoryExistsAsync(categoryId, cancellationToken))
            {
                return new ProductCreateResult(ProductCreateStatus.CategoryNotFound);
            }
        }
        else if (await _products.CategoryNameExistsAsync(categoryName!, cancellationToken))
        {
            return new ProductCreateResult(ProductCreateStatus.CategoryAlreadyExists);
        }

        if (await _products.ProductExistsAsync(
                request.CategoryId,
                categoryName,
                brand,
                model,
                cancellationToken))
        {
            return new ProductCreateResult(ProductCreateStatus.DuplicateProduct);
        }

        var product = new Product
        {
            ProductCategoryId = request.CategoryId ?? 0,
            Category = request.CategoryId.HasValue
                ? null!
                : new ProductCategory { Name = categoryName! },
            Brand = brand,
            Model = model,
            ImageFileName = request.ImageFileName,
            IsActive = true,
            Variants = request.Variants.Select(variant =>
            {
                var newVariant = new ProductVariant
                {
                    OptionLabel = variant.OptionLabel.Trim(),
                    Size = Normalize(variant.Size),
                    Gender = Normalize(variant.Gender),
                    EquipmentType = Normalize(variant.EquipmentType),
                    ThicknessMm = variant.ThicknessMm,
                    VolumeLiters = variant.VolumeLiters,
                    FirstStage = Normalize(variant.FirstStage),
                    SecondStage = Normalize(variant.SecondStage),
                    Octopus = Normalize(variant.Octopus),
                    DailyRate = variant.DailyRate,
                    IsActive = true
                };

                for (var index = 0; index < variant.InitialUnitCount; index++)
                {
                    newVariant.EquipmentUnits.Add(new EquipmentUnit
                    {
                        AssetTag = $"DD-{Guid.NewGuid():N}",
                        Status = EquipmentUnitStatus.Active
                    });
                }

                return newVariant;
            }).ToList()
        };

        var savedProduct = await _products.AddProductAsAdminAsync(product, cancellationToken);
        return savedProduct is null
            ? new ProductCreateResult(ProductCreateStatus.SaveConflict)
            : new ProductCreateResult(ProductCreateStatus.Created, savedProduct);
    }

    public async Task<ProductDeleteResult> DeleteProductAsAdminAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var outcome = await _products.DeleteProductAsAdminAsync(productId, cancellationToken);
        var status = outcome.Status switch
        {
            ProductDeletionStatus.Deleted => ProductDeleteStatus.Deleted,
            ProductDeletionStatus.Archived => ProductDeleteStatus.Archived,
            _ => ProductDeleteStatus.NotFound
        };

        return new ProductDeleteResult(status, outcome.UnusedImageFileName);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
