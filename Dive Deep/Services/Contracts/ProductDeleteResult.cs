namespace Dive_Deep.Services.Contracts;

public enum ProductDeleteStatus
{
    Deleted,
    Archived,
    NotFound
}

public sealed record ProductDeleteResult(
    ProductDeleteStatus Status,
    string? UnusedImageFileName = null);
