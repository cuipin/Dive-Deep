using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dive_Deep.Models.ProduktTypes
{
    public enum FinStørrelse
    {
        XS,
        S,
        M,
        L,
        XL,
    }

    public class Finner : Product
    {
        public string? Model { get; set; }
        public List<FinStørrelse> Størrelse { get; set; } = new();
    }
}