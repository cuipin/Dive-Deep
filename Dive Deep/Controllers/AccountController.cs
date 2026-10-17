using Dive_Deep.Data;
using Dive_Deep.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;

    public AccountController(UserManager<ApplicationUser> users) => _users = users;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        return View(new AccountDashboardViewModel
        {
            UserName = user.UserName ?? user.Email ?? "Bruger",
            Email = user.Email,
            IsAdmin = await _users.IsInRoleAsync(user, "Admin")
        });
    }
}
