namespace Dive_Deep.Models;

public class Product
{
    public int ProductId { get; set; }
    public int ProductCategoryId { get; set; }
    public ProductCategory Category { get; set; } = null!;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? ImageFileName { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
