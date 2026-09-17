using Microsoft.AspNetCore.Identity;
using Dive_Deep.Models;

namespace Dive_Deep.Data
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Booking>? Bookings { get; set; }
    }
}