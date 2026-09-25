using Dive_Deep.Models;

namespace Dive_Deep.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int PrisPrDag { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now.AddHours(1);
    }
}