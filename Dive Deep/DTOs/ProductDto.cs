using Dive_Deep.Models;
using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.DTOs;

public class ProductDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductCategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string? ImageFileName { get; set; }

    public bool IsActive { get; set; } = true;

    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class ProductVariantDto
{
    public int ProductVariantId { get; set; }
    public int ProductId { get; set; }
    public string OptionLabel { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Gender { get; set; }
    public string? EquipmentType { get; set; }
    public decimal? ThicknessMm { get; set; }
    public decimal? VolumeLiters { get; set; }
    public string? FirstStage { get; set; }
    public string? SecondStage { get; set; }
    public string? Octopus { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public int AvailableUnits { get; set; }
}