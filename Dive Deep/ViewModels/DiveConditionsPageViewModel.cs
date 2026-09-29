using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.ViewModels;

public sealed class DiveConditionsPageViewModel
{
    [Required(ErrorMessage = "Skriv en dansk lokation eller koordinater.")]
    [StringLength(120, ErrorMessage = "Lokationen må højst være 120 tegn.")]
    [Display(Name = "Lokation")]
    public string Location { get; set; } = string.Empty;

    public DiveConditionsReport? Report { get; set; }

    public string? ErrorMessage { get; set; }
}

public sealed record DiveConditionsReport(
    string ResolvedLocation,
    DateTime ObservationTime,
    double Latitude,
    double Longitude,
    double WindSpeedMetersPerSecond,
    double WaveHeightMeters,
    double PrecipitationMillimeters,
    bool IsThunderstorm,
    double WaterTemperatureCelsius,
    bool IsSuitableForDiving,
    IReadOnlyList<string> UnsuitableReasons,
    string WetsuitRecommendation);
