using Microsoft.AspNetCore.Identity;
using AccountManagementSystem.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AccountManagementSystem.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Accountant", "Viewer" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() });
                    Console.WriteLine($"Role '{roleName}' created.");
                }
            }

            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };
                // Attempt to create the user with a password
                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Default admin user 'admin@example.com' created and assigned 'Admin' role.");
                }
                else
                {
                    Console.WriteLine("Error creating admin user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin user 'admin@example.com' already exists but was assigned 'Admin' role.");
                }
            }
        }
    }
}
