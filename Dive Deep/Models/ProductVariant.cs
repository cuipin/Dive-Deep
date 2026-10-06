namespace Dive_Deep.Models;

/// <summary>A selectable catalog option. Physical stock is tracked by EquipmentUnit.</summary>
public class ProductVariant
{
    public int ProductVariantId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
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
    public ICollection<EquipmentUnit> EquipmentUnits { get; set; } = new List<EquipmentUnit>();
    public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
