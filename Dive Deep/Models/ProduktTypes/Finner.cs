using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dive_Deep.Models.ProduktTypes
{
    public class Finner : Product
    {
        public string ? Model { get; set; }
        public string ? Størrelse { get; set; }
    }
}
