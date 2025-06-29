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
public class ManageRolesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ManageRolesModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }
    public IList<string> UserRoles { get; set; } = new List<string>();
    public List<ApplicationRole> AllRoles { get; set; } = new List<ApplicationRole>();

    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return NotFound("User ID cannot be null or empty.");
        }

        User = await _userManager.FindByIdAsync(userId);

        if (User == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        UserId = userId;
        UserRoles = await _userManager.GetRolesAsync(User);
        AllRoles = _roleManager.Roles.OrderBy(r => r.Name).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string[] selectedRoles)
    {
        User = await _userManager.FindByIdAsync(UserId);
        if (User == null)
        {
            ErrorMessage = $"Unable to load user with ID '{UserId}'.";
            return Page();
        }

        var currentRoles = await _userManager.GetRolesAsync(User);
        var rolesToRemove = currentRoles.Except(selectedRoles).ToList();
        var rolesToAdd = selectedRoles.Except(currentRoles).ToList();

        IdentityResult result;

        if (rolesToRemove.Any())
        {
            result = await _userManager.RemoveFromRolesAsync(User, rolesToRemove);
            if (!result.Succeeded)
            {
                ErrorMessage = "Error removing roles: " + string.Join("; ", result.Errors.Select(e => e.Description));
                return Page();
            }
        }

        if (rolesToAdd.Any())
        {
            result = await _userManager.AddToRolesAsync(User, rolesToAdd);
            if (!result.Succeeded)
            {
                ErrorMessage = "Error adding roles: " + string.Join("; ", result.Errors.Select(e => e.Description));
                return Page();
            }
        }

        StatusMessage = $"Roles for user '{User.Email}' updated successfully.";
        UserRoles = await _userManager.GetRolesAsync(User); 
        AllRoles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        return RedirectToPage("./Index"); 
    }
}