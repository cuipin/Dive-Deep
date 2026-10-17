using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dive_Deep.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dive_Deep.Services;

public sealed class JwtAccessTokenService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> options) : IAccessTokenService
{
    public async Task<AccessTokenResult> CreateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var jwt = options.Value;
        var roles = await userManager.GetRolesAsync(user);
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(jwt.AccessTokenLifetimeMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // JWT'en er det mobile loginbevis. SigningKey'en gør, at API'et kan opdage falske tokens.
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            roles.ToArray());
    }
}
