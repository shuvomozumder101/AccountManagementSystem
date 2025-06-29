using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public Dictionary<string, IList<string>> UserRoles { get; set; } = new Dictionary<string, IList<string>>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Users = _userManager.Users.OrderBy(u => u.Email).ToList();

        foreach (var user in Users)
        {
            UserRoles[user.Id] = await _userManager.GetRolesAsync(user);
        }
    }
}
