namespace Dive_Deep.Persistence;

public enum ProductDeletionStatus
{
    Deleted,
    Archived,
    NotFound
}

public sealed record ProductDeletionOutcome(
    ProductDeletionStatus Status,
    string? UnusedImageFileName = null);
