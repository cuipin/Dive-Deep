using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dive_Deep.Data;
using Dive_Deep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Dive_Deep.Controllers
{
    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password);

    // Login til mobilappen: returnerer et JWT, som sendes som "Authorization: Bearer <token>".
    // Websitet bruger stadig Identity-cookies og er upåvirket.
    [Route("api/auth")]
    [ApiController]
    public class ApiAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;
        private readonly IConfiguration _config;

        public ApiAuthController(
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn,
            IConfiguration config)
        {
            _users = users;
            _signIn = signIn;
            _config = config;
        }

        // POST: api/auth/register   body: { "email": "...", "password": "..." }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email og adgangskode skal udfyldes.");
            }

            if (await _users.FindByEmailAsync(request.Email) != null)
            {
                return Conflict("Der findes allerede en bruger med den email.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _users.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok(new { message = "Bruger oprettet. Log ind med /api/auth/login.", email = user.Email });
        }

        // POST: api/auth/login   body: { "email": "...", "password": "..." }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email og adgangskode skal udfyldes.");
            }

            var user = await _users.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized("Forkert email eller adgangskode.");
            }

            var result = await _signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                return Unauthorized("Kontoen er midlertidigt låst. Prøv igen senere.");
            }
            if (!result.Succeeded)
            {
                return Unauthorized("Forkert email eller adgangskode.");
            }

            var roles = await _users.GetRolesAsync(user);

            // Samme claim-typer som controllerne læser (MapInboundClaims = false i Program.cs).
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
                new(ClaimTypes.Email, user.Email ?? "")
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                expiresAtUtc = expires,
                email = user.Email,
                roles
            });
        }
    }
}