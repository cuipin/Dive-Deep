namespace Dive_Deep.Services;

/// <summary>Indstillingerne ligger i appsettings, mens selve SigningKey ligger i User Secrets.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; init; } = 60;
    public string SigningKey { get; init; } = string.Empty;
}
