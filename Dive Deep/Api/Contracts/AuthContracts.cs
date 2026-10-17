using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.Api.Contracts;

public sealed class RegisterRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    ApiUserResponse User);

public sealed record ApiUserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyList<string> Roles);
