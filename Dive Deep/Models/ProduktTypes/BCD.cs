namespace Dive_Deep.Models.ProduktTypes
{
    public enum BCDStørrelse
    {
        S,
        M,
        L,
    }

    public class BCD : Product
    {
        public string? Model { get; set; }
        public List<BCDStørrelse> Størrelse { get; set; } = new();
    }
}