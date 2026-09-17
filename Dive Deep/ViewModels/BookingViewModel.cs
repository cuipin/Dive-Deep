using Dive_Deep.Models;

namespace Dive_Deep.ViewModels
{
    public class BookingViewModel
    {
        public Booking Booking { get; set; } = new Booking();
        public List<Product> Rooms { get; set; } = new List<Product>();
    }
}