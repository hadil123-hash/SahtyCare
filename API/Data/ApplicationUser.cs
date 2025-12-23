using Microsoft.AspNetCore.Identity;

namespace Sahty.Shared.Data;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}
