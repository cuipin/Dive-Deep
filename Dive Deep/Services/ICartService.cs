using Dive_Deep.Models;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Services;

public interface ICartService
{
    Task<Cart> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartOperationResult> AddItemAsync(
        string userId,
        int productVariantId,
        int quantity,
        CancellationToken cancellationToken = default);
    Task<CartOperationResult> AddPackageAsync(
        string userId,
        RentalPackageType packageType,
        IReadOnlyCollection<int> productVariantIds,
        int quantity,
        CancellationToken cancellationToken = default);
    Task<CartOperationResult> UpdateRentalPeriodAsync(
        string userId,
        int cartItemId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);
    Task<CartOperationResult> RemoveItemAsync(
        string userId,
        int cartItemId,
        CancellationToken cancellationToken = default);
    Task ClearAsync(string userId, CancellationToken cancellationToken = default);
}
