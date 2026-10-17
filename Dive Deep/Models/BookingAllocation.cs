namespace Dive_Deep.Models;

public class BookingAllocation
{
    public int BookingAllocationId { get; set; }
    public int BookingItemId { get; set; }
    public BookingItem BookingItem { get; set; } = null!;
    public int EquipmentUnitId { get; set; }
    public EquipmentUnit EquipmentUnit { get; set; } = null!;
}
