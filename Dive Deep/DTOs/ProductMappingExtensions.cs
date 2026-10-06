using Dive_Deep.Models;

namespace Dive_Deep.DTOs;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product) => new()
    {
        Id = product.ProductId,
        ProductCategoryId = product.ProductCategoryId,
        Brand = product.Brand,
        Model = product.Model,
        ImageFileName = product.ImageFileName,
        IsActive = product.IsActive
    };

    public static Product ToEntity(this ProductDto dto) => new()
    {
        ProductId = dto.Id,
        ProductCategoryId = dto.ProductCategoryId,
        Brand = dto.Brand.Trim(),
        Model = dto.Model.Trim(),
        ImageFileName = dto.ImageFileName,
        IsActive = dto.IsActive
    };
}