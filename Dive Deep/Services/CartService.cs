using Dive_Deep.Models;
using Dive_Deep.Persistence;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _carts;
    private readonly IProductRepository _products;

    public CartService(ICartRepository carts, IProductRepository products)
    {
        _carts = carts;
        _products = products;
    }

    public Task<Cart> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        _carts.GetOrCreateForUserAsync(userId, cancellationToken);

    public async Task<CartOperationResult> AddItemAsync(
        string userId,
        int productVariantId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateUserAndQuantity(userId, quantity);
        if (validation is not null)
        {
            return CartOperationResult.Failure(validation);
        }

        if (productVariantId <= 0)
        {
            return CartOperationResult.Failure("Vælg venligst et produkt.");
        }

        await _carts.GetOrCreateForUserAsync(userId, cancellationToken);
        var added = await _carts.AddItemsAsync(
            userId,
            [new CartLineRequest(productVariantId, quantity)],
            cancellationToken);

        return added
            ? CartOperationResult.Success()
            : CartOperationResult.Failure("Produktet findes ikke eller kan ikke lejes.");
    }

    public async Task<CartOperationResult> AddPackageAsync(
        string userId,
        RentalPackageType packageType,
        IReadOnlyCollection<int> productVariantIds,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateUserAndQuantity(userId, quantity);
        if (validation is not null)
        {
            return CartOperationResult.Failure(validation);
        }

        var expectedCount = packageType switch
        {
            RentalPackageType.CompleteDiveSet => 7,
            RentalPackageType.CompleteSnorkelSet => 3,
            _ => 0
        };

        if (expectedCount == 0
            || productVariantIds.Count != expectedCount
            || productVariantIds.Distinct().Count() != expectedCount)
        {
            return CartOperationResult.Failure("Vælg én gyldig variant for hver del af sættet.");
        }

        var variants = await _products.GetVariantsByIdsAsync(productVariantIds, cancellationToken);
        if (variants.Count != expectedCount || !IsCompletePackage(packageType, variants))
        {
            return CartOperationResult.Failure("Sættet mangler en udstyrskategori eller indeholder en forkert variant.");
        }

        // Datasheetet samler maske/snorkel i én kategori, så det bliver én kataloglinje.
        var cartLines = variants.Select(variant =>
            new CartLineRequest(variant.ProductVariantId, quantity)).ToArray();

        await _carts.GetOrCreateForUserAsync(userId, cancellationToken);
        var added = await _carts.AddItemsAsync(userId, cartLines, cancellationToken);

        return added
            ? CartOperationResult.Success()
            : CartOperationResult.Failure("En eller flere produktvarianter findes ikke længere.");
    }

    private static bool IsCompletePackage(
        RentalPackageType packageType,
        IReadOnlyCollection<ProductVariant> variants)
    {
        bool HasSingleCategory(string category) => variants.Count(variant =>
            string.Equals(variant.Product.Category.Name, category, StringComparison.OrdinalIgnoreCase)) == 1;
        bool HasSingleEquipmentType(string equipmentType) => variants.Count(variant =>
            string.Equals(variant.Product.Category.Name, ProductCategoryNames.MasksAndSnorkels, StringComparison.OrdinalIgnoreCase)
            && string.Equals(variant.EquipmentType, equipmentType, StringComparison.OrdinalIgnoreCase)) == 1;

        var hasMaskAndSnorkel = HasSingleEquipmentType("Maske") && HasSingleEquipmentType("Snorkel");
        if (packageType == RentalPackageType.CompleteSnorkelSet)
        {
            return hasMaskAndSnorkel && HasSingleCategory(ProductCategoryNames.Fins);
        }

        return hasMaskAndSnorkel
            && HasSingleCategory(ProductCategoryNames.Bcd)
            && HasSingleCategory(ProductCategoryNames.Wetsuits)
            && HasSingleCategory(ProductCategoryNames.Regulators)
            && HasSingleCategory(ProductCategoryNames.Tanks)
            && HasSingleCategory(ProductCategoryNames.Fins);
    }

    public async Task<CartOperationResult> UpdateRentalPeriodAsync(
        string userId,
        int cartItemId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateUserAndQuantity(userId, 1);
        if (validation is not null)
        {
            return CartOperationResult.Failure(validation);
        }

        validation = ValidateRentalPeriod(startTime, endTime);
        if (validation is not null)
        {
            return CartOperationResult.Failure(validation);
        }

        var updated = await _carts.UpdateRentalPeriodAsync(
            userId,
            cartItemId,
            startTime.ToUniversalTime(),
            endTime.ToUniversalTime(),
            cancellationToken);

        return updated
            ? CartOperationResult.Success()
            : CartOperationResult.Failure("Produktet blev ikke fundet i din kurv.");
    }

    public async Task<CartOperationResult> RemoveItemAsync(
        string userId,
        int cartItemId,
        CancellationToken cancellationToken = default)
    {
        var removed = await _carts.RemoveItemAsync(userId, cartItemId, cancellationToken);
        return removed
            ? CartOperationResult.Success()
            : CartOperationResult.Failure("Produktet blev ikke fundet i din kurv.");
    }

    public Task ClearAsync(string userId, CancellationToken cancellationToken = default) =>
        _carts.ClearForUserAsync(userId, cancellationToken);

    private static string? ValidateUserAndQuantity(string userId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return "Du skal være logget ind for at bruge kurven.";
        }

        if (quantity < 1)
        {
            return "Antallet skal være mindst 1.";
        }

        return null;
    }

    private static string? ValidateRentalPeriod(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        if (startTime < DateTimeOffset.UtcNow)
        {
            return "Starttidspunktet må ikke ligge i fortiden.";
        }

        if (endTime <= startTime)
        {
            return "Sluttidspunktet skal ligge efter starttidspunktet.";
        }

        return null;
    }
}
