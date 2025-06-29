using Microsoft.AspNetCore.Identity;

namespace AccountManagementSystem.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
