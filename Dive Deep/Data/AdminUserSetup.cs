using Microsoft.AspNetCore.Identity;

namespace Dive_Deep.Data;

/// <summary>Explicit one-time role assignment; never promotes users during normal startup.</summary>
public static class AdminUserSetup
{
    public const string AdminRole = "Admin";

    public static async Task GrantAdminAsync(IServiceProvider services, string email)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRole));
            EnsureSucceeded(roleResult, "Kunne ikke oprette Admin-rollen.");
        }

        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            throw new InvalidOperationException(
                $"Der findes ingen bruger med e-mailadressen '{email}'. Opret kontoen først, og kør kommandoen igen.");
        }

        if (await userManager.IsInRoleAsync(user, AdminRole))
        {
            return;
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, AdminRole);
        EnsureSucceeded(addRoleResult, "Kunne ikke føje brugeren til Admin-rollen.");
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"{message} {errors}");
        }
    }
}
