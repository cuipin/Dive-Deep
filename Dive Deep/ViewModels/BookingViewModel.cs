using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.ViewModels;

public class BookingViewModel
{
    public BookingFormInput Booking { get; set; } = new();

    public List<ProductChoiceViewModel> Products { get; set; } = new();

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }
}

public class BookingFormInput
{
    public int BookingId { get; set; }

    public int BookingItemId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Vælg venligst et produkt.")]
    [Display(Name = "Udstyr")]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Antallet skal være mellem 1 og 100.")]
    [Display(Name = "Antal")]
    public int Quantity { get; set; } = 1;

    [Required]
    [Display(Name = "Startdato og -tid")]
    public DateTime StartTime { get; set; }

    [Required]
    [Display(Name = "Slutdato og -tid")]
    public DateTime EndTime { get; set; }
}

public class ProductChoiceViewModel
{
    public int ProductId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Mærke { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string OptionLabel { get; set; } = string.Empty;

    public string DisplayName => $"{Category} - {Mærke} {Model} ({OptionLabel})";
}
