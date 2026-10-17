namespace Dive_Deep.Services.Contracts;

public sealed record CartOperationResult(bool Succeeded, string? ErrorMessage = null)
{
    public static CartOperationResult Success() => new(true);

    public static CartOperationResult Failure(string message) => new(false, message);
}
