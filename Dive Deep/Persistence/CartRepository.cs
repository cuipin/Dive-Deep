using Dive_Deep.Data;
using Dive_Deep.Models;
using Dive_Deep.Services.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dive_Deep.Persistence;

public class CartRepository : ICartRepository
{
    private readonly Dive_DeepContext _DiveDeepContext;

    public CartRepository(Dive_DeepContext Database) => _DiveDeepContext = Database;

    public async Task<Cart> GetOrCreateForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var existing = await LoadCartAsync(userId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var cart = new Cart { UserId = userId };
        _DiveDeepContext.Carts.Add(cart);

        try
        {
            await _DiveDeepContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConflict(exception))
        {
            _DiveDeepContext.Entry(cart).State = EntityState.Detached;
            return (await LoadCartAsync(userId, cancellationToken))!;
        }

        return (await LoadCartAsync(userId, cancellationToken))!;
    }

    public async Task<bool> AddItemsAsync(
        string userId,
        IReadOnlyCollection<CartLineRequest> items,
        CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return false;
        }

        var cart = await _DiveDeepContext.Carts
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (cart is null)
        {
            return false;
        }

        var variantIds = items.Select(item => item.ProductVariantId).Distinct().ToArray();
        var activeVariantIds = await _DiveDeepContext.ProductVariants
            .Where(variant => variantIds.Contains(variant.ProductVariantId)
                && variant.IsActive
                && variant.Product.IsActive)
            .Select(variant => variant.ProductVariantId)
            .ToListAsync(cancellationToken);

        if (activeVariantIds.Count != variantIds.Length)
        {
            return false;
        }

        foreach (var request in items)
        {
            cart.Items.Add(new CartItem
            {
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity
            });
        }

        await _DiveDeepContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateRentalPeriodAsync(
        string userId,
        int cartItemId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        var item = await _DiveDeepContext.CartItems
            .SingleOrDefaultAsync(candidate =>
                candidate.CartItemId == cartItemId && candidate.Cart.UserId == userId,
                cancellationToken);

        if (item is null)
        {
            return false;
        }

        item.StartTime = startTime;
        item.EndTime = endTime;
        await _DiveDeepContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveItemAsync(
        string userId,
        int cartItemId,
        CancellationToken cancellationToken = default)
    {
        var cartItem = await _DiveDeepContext.CartItems
            .SingleOrDefaultAsync(item =>
                item.CartItemId == cartItemId && item.Cart.UserId == userId,
                cancellationToken);

        if (cartItem is null)
        {
            return false;
        }

        _DiveDeepContext.CartItems.Remove(cartItem);
        await _DiveDeepContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ClearForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var items = await _DiveDeepContext.CartItems
            .Where(item => item.Cart.UserId == userId)
            .ToListAsync(cancellationToken);

        _DiveDeepContext.CartItems.RemoveRange(items);
        await _DiveDeepContext.SaveChangesAsync(cancellationToken);
    }

    private Task<Cart?> LoadCartAsync(string userId, CancellationToken cancellationToken) =>
        _DiveDeepContext.Carts
            .AsNoTracking()
            .Where(cart => cart.UserId == userId)
            .Include(cart => cart.Items)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant.Product)
                        .ThenInclude(product => product.Category)
            .SingleOrDefaultAsync(cancellationToken);

    private static bool IsUniqueConflict(DbUpdateException exception) =>
        exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
}