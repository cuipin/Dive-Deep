using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.ViewModels;

public class BookingViewModel
{
    public BookingFormInput Booking { get; set; } = new();

    public List<ProductChoiceViewModel> Products { get; set; } = new();
}

public class BookingFormInput
{
    public int BookingId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Vælg venligst et produkt.")]
    [Display(Name = "Udstyr")]
    public int ProductId { get; set; }

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
