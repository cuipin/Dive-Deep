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

    public string? ImageFileName { get; set; }

    public bool IsActive { get; set; } = true;
}