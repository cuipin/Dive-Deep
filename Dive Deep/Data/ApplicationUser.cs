using System.ComponentModel.DataAnnotations;
using Dive_Deep.Models;
using Microsoft.AspNetCore.Identity;

namespace Dive_Deep.Data;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public Cart? Cart { get; set; }
}
