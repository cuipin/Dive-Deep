using Dive_Deep.Data;

namespace Dive_Deep.Services;

public interface IAccessTokenService
{
    Task<AccessTokenResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}

public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<string> Roles);
