using Dive_Deep.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dive_Deep.Data
{
    public class Dive_DeepContext : IdentityDbContext<ApplicationUser>
    {
        public Dive_DeepContext(DbContextOptions<Dive_DeepContext> options)
            : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
    }
}