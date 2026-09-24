using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}