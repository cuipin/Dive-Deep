using Dive_Deep.Models;

namespace Dive_Deep.Services.Contracts;

public enum ProductCreateStatus
{
    Created,
    InvalidRequest,
    CategoryNotFound,
    CategoryAlreadyExists,
    DuplicateProduct,
    SaveConflict
}

public sealed record ProductCreateResult(ProductCreateStatus Status, Product? Product = null)
{
    public bool Succeeded => Status == ProductCreateStatus.Created && Product is not null;
}
