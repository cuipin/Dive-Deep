namespace Dive_Deep.Models;

public enum EquipmentUnitStatus
{
    Active,
    Maintenance,
    Retired
}

public class EquipmentUnit
{
    public int EquipmentUnitId { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public string AssetTag { get; set; } = string.Empty;
    public EquipmentUnitStatus Status { get; set; } = EquipmentUnitStatus.Active;
    public ICollection<BookingAllocation> Allocations { get; set; } = new List<BookingAllocation>();
}
