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
        CategoryName = product.Category?.Name ?? string.Empty,
        ImageFileName = product.ImageFileName,
        IsActive = product.IsActive,
        Variants = product.Variants?.Select(v => v.ToDto()).ToList() ?? new()
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

    public static ProductVariant ToEntity(this ProductVariantDto dto) => new()
    {
        ProductVariantId = dto.ProductVariantId,
        ProductId = dto.ProductId,
        OptionLabel = dto.OptionLabel.Trim(),
        Size = dto.Size?.Trim(),
        Gender = dto.Gender?.Trim(),
        EquipmentType = dto.EquipmentType?.Trim(),
        ThicknessMm = dto.ThicknessMm,
        VolumeLiters = dto.VolumeLiters,
        FirstStage = dto.FirstStage?.Trim(),
        SecondStage = dto.SecondStage?.Trim(),
        Octopus = dto.Octopus?.Trim(),
        DailyRate = dto.DailyRate,
        IsActive = dto.IsActive,
    };

    public static ProductVariantDto ToDto(this ProductVariant variant)
    {
        var activeUnits = variant.EquipmentUnits?
            .Count(u => u.Status == EquipmentUnitStatus.Active) ?? 0;

        return new ProductVariantDto
        {
            ProductVariantId = variant.ProductVariantId,
            ProductId = variant.ProductId,
            OptionLabel = variant.OptionLabel,
            Size = variant.Size,
            Gender = variant.Gender,
            EquipmentType = variant.EquipmentType,
            ThicknessMm = variant.ThicknessMm,
            VolumeLiters = variant.VolumeLiters,
            FirstStage = variant.FirstStage,
            SecondStage = variant.SecondStage,
            Octopus = variant.Octopus,
            DailyRate = variant.DailyRate,
            IsActive = variant.IsActive,
            AvailableUnits = activeUnits,
            IsAvailable = variant.IsActive && activeUnits > 0
        };
    }
}