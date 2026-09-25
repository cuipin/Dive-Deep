namespace Dive_Deep.Models.ProduktTypes
{
    public enum DykkerdragterStørrelse
    {
        S,
        M,
        L,
    }

    public enum DykkedragterKøn
    {
        Herre,
        Dame,
        Unisex,
    }

    public class Dykkerdragter : Product
    {
        public string? Model { get; set; }
        public List<DykkerdragterStørrelse> Størrelse { get; set; } = new();
        public string? Type { get; set; }
        public List<DykkedragterKøn> Køn { get; set; } = new();
        public double? Tykkelse { get; set; }
    }
}