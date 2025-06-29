using Microsoft.AspNetCore.Identity;

namespace AccountManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
