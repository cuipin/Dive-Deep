using Dive_Deep.Api;
using Dive_Deep.Api.Contracts;
using Dive_Deep.Data;
using Dive_Deep.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAccessTokenService accessTokens) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        // Identity står stadig for password-hashing og reglerne; API'et udsteder kun et token bagefter.
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(ModelState);
        }

        return Ok(await CreateResponseAsync(user, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return Unauthorized(new { message = "E-mail eller adgangskode er forkert." });
        }

        // CheckPasswordSignInAsync tjekker Identity-regler uden at oprette MVC's cookie-login.
        var signIn = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signIn.Succeeded)
        {
            return Unauthorized(new { message = "E-mail eller adgangskode er forkert." });
        }

        return Ok(await CreateResponseAsync(user, cancellationToken));
    }

    [Authorize(Policy = ApiPolicies.MobileApi)]
    [HttpGet("me")]
    public async Task<ActionResult<ApiUserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        cancellationToken.ThrowIfCancellationRequested();
        var roles = await userManager.GetRolesAsync(user);
        return Ok(ToUserResponse(user, roles.ToArray()));
    }

    private async Task<AuthResponse> CreateResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var token = await accessTokens.CreateAsync(user, cancellationToken);
        return new AuthResponse(
            token.AccessToken,
            "Bearer",
            token.ExpiresAtUtc,
            ToUserResponse(user, token.Roles));
    }

    private static ApiUserResponse ToUserResponse(ApplicationUser user, IReadOnlyList<string> roles) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email ?? string.Empty,
        roles);
}
