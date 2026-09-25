using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Dive_Deep.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}