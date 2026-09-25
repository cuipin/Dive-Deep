namespace Dive_Deep.Models.ProduktTypes;

public class Dykkerdragter : ProductDisplayModel
{
    public string Model { get; set; } = string.Empty;
    public string Størrelse { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Køn { get; set; } = string.Empty;
    public decimal? Tykkelse { get; set; }
}
