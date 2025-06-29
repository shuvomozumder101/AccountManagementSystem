using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; 
using AccountManagementSystem.DataAccess;
using AccountManagementSystem.Models; 

namespace AccountManagementSystem.Pages.ChartOfAccounts;

[Authorize(Roles = "Admin,Accountant")]
public class CreateModel : PageModel
{
    private readonly ChartOfAccountsDAL _chartOfAccountsDAL;

    public CreateModel(ChartOfAccountsDAL chartOfAccountsDAL)
    {
        _chartOfAccountsDAL = chartOfAccountsDAL;
    }

    [BindProperty]
    public ChartOfAccountsViewModel Account { get; set; } = new ChartOfAccountsViewModel();

    public SelectList ParentAccountsSelectList { get; set; } = new SelectList(new List<SelectListItem>());

    [TempData]
    public string? StatusMessage { get; set; }

    public IActionResult OnGet()
    {
        PopulateParentAccountsDropdown();
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            PopulateParentAccountsDropdown();
            return Page();
        }

        try
        {

            _chartOfAccountsDAL.ManageChartOfAccounts(
                "Insert",
                accountId: null,
                accountCode: Account.AccountCode,
                accountName: Account.AccountName,
                accountType: Account.AccountType,
                parentAccountId: Account.ParentAccountId,
                isActive: Account.IsActive
            );

            StatusMessage = $"Account '{Account.AccountName}'.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating account: {ex.Message}");
            PopulateParentAccountsDropdown();
            return Page();
        }
    }

    private void PopulateParentAccountsDropdown()
    {
        var parentAccounts = _chartOfAccountsDAL.ManageChartOfAccounts("SelectParents");
        ParentAccountsSelectList = new SelectList(
            parentAccounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text = $"{a.AccountCode} - {a.AccountName}"
            }).ToList(),
            "Value", "Text");
    }
}
