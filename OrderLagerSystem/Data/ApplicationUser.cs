using Microsoft.AspNetCore.Identity;

namespace OrderLagerSystem.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}