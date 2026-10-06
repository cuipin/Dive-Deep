namespace Dive_Deep.Models.ProduktTypes;

public abstract class ProductDisplayModel
{
    public int ProductId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Mærke { get; set; } = string.Empty;
    public decimal PrisPrDag { get; set; }
}
