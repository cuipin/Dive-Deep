using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dive_Deep.ViewModels;

public sealed class AdminProductCreateViewModel
{
    [Display(Name = "Eksisterende kategori")]
    public int? CategoryId { get; set; }

    [StringLength(80, ErrorMessage = "Kategorien må højst være 80 tegn.")]
    [Display(Name = "Ny kategori")]
    public string? NewCategoryName { get; set; }

    [Required(ErrorMessage = "Skriv produktets mærke.")]
    [StringLength(120, ErrorMessage = "Mærket må højst være 120 tegn.")]
    [Display(Name = "Mærke")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Skriv produktets modelnavn.")]
    [StringLength(160, ErrorMessage = "Modelnavnet må højst være 160 tegn.")]
    [Display(Name = "Model")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vælg et produktbillede.")]
    [Display(Name = "Produktbillede")]
    public IFormFile? Image { get; set; }

    [MinLength(1, ErrorMessage = "Tilføj mindst én variant.")]
    public List<AdminProductVariantInputViewModel> Variants { get; set; } =
        [new AdminProductVariantInputViewModel()];

    public List<SelectListItem> Categories { get; set; } = [];
}

public sealed class AdminProductVariantInputViewModel
{
    [Required(ErrorMessage = "Hver variant skal have et navn, for eksempel størrelse M.")]
    [StringLength(160)]
    [Display(Name = "Variant")]
    public string OptionLabel { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Størrelse")]
    public string? Size { get; set; }

    [StringLength(40)]
    [Display(Name = "Køn")]
    public string? Gender { get; set; }

    [StringLength(60)]
    [Display(Name = "Udstyrstype")]
    public string? EquipmentType { get; set; }

    [Range(typeof(decimal), "0", "1000", ErrorMessage = "Tykkelsen skal være 0 eller højere.")]
    [Display(Name = "Tykkelse i mm")]
    public decimal? ThicknessMm { get; set; }

    [Range(typeof(decimal), "0", "10000", ErrorMessage = "Volumen skal være 0 eller højere.")]
    [Display(Name = "Volumen i liter")]
    public decimal? VolumeLiters { get; set; }

    [StringLength(100)]
    [Display(Name = "Første trin")]
    public string? FirstStage { get; set; }

    [StringLength(100)]
    [Display(Name = "Andet trin")]
    public string? SecondStage { get; set; }

    [StringLength(100)]
    [Display(Name = "Octopus")]
    public string? Octopus { get; set; }

    [Range(typeof(decimal), "0.01", "50000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "Dagsprisen skal være større end 0 kr.")]
    [Display(Name = "Dagspris")]
    public decimal DailyRate { get; set; }

    [Range(0, 5000, ErrorMessage = "Antal skal være mellem 0 og 5.000.")]
    [Display(Name = "Antal fysiske enheder")]
    public int InitialUnitCount { get; set; }
}
