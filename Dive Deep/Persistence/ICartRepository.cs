using Dive_Deep.Models;
using Dive_Deep.Services.Contracts;

namespace Dive_Deep.Persistence;

public interface ICartRepository
{
    Task<Cart> GetOrCreateForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> AddItemsAsync(
        string userId,
        IReadOnlyCollection<CartLineRequest> items,
        CancellationToken cancellationToken = default);
    Task<bool> UpdateRentalPeriodAsync(
        string userId,
        int cartItemId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);
    Task<bool> RemoveItemAsync(string userId, int cartItemId, CancellationToken cancellationToken = default);
    Task ClearForUserAsync(string userId, CancellationToken cancellationToken = default);
}
