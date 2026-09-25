namespace Dive_Deep.Models
{
    public class Product
    {
        public virtual string DisplayName => $"{Category} - {Mærke}";
        public int ProductId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Mærke { get; set; } = string.Empty;
        public int PrisPrDag { get; set; }
    }
}