namespace Dive_Deep.ViewModels;

public sealed class AccountDashboardViewModel
{
    public required string UserName { get; init; }
    public string? Email { get; init; }
    public bool IsAdmin { get; init; }
}
