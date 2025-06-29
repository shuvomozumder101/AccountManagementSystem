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
public class EditModel : PageModel
{
    private readonly ChartOfAccountsDAL _chartOfAccountsDAL;

    public EditModel(ChartOfAccountsDAL chartOfAccountsDAL)
    {
        _chartOfAccountsDAL = chartOfAccountsDAL;
    }

    [BindProperty]
    public ChartOfAccountsViewModel Account { get; set; } = new ChartOfAccountsViewModel();

    public SelectList ParentAccountsSelectList { get; set; } = new SelectList(new List<SelectListItem>());

    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(int? id)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Accountant"))
        {
            return Forbid();
        }

        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var accountFromDb = _chartOfAccountsDAL.ManageChartOfAccounts("Select", id).FirstOrDefault();

            if (accountFromDb == null)
            {
                return NotFound();
            }


            Account = new ChartOfAccountsViewModel
            {
                AccountId = accountFromDb.AccountId,
                AccountCode = accountFromDb.AccountCode,
                AccountName = accountFromDb.AccountName,
                AccountType = accountFromDb.AccountType,
                ParentAccountId = accountFromDb.ParentAccountId,
                IsActive = accountFromDb.IsActive,
                CreatedDate = accountFromDb.CreatedDate
            };

            PopulateParentAccountsDropdown(Account.AccountId);
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading account: {ex.Message}";
            return Page();
        }
    }

    public IActionResult OnPost()
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Accountant"))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            PopulateParentAccountsDropdown(Account.AccountId);
            return Page();
        }

        try
        {
            _chartOfAccountsDAL.ManageChartOfAccounts(
                "Update",
                accountId: Account.AccountId,
                accountCode: Account.AccountCode,
                accountName: Account.AccountName,
                accountType: Account.AccountType,
                parentAccountId: Account.ParentAccountId,
                isActive: Account.IsActive
            );

            StatusMessage = $"Account '{Account.AccountName}' updated successfully.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error updating account: {ex.Message}");
            PopulateParentAccountsDropdown(Account.AccountId);
            return Page();
        }
    }

    private void PopulateParentAccountsDropdown(int? excludeAccountId = null)
    {
        var parentAccounts = _chartOfAccountsDAL.ManageChartOfAccounts("SelectParents");

        if (excludeAccountId.HasValue)
        {
            var allAccounts = _chartOfAccountsDAL.ManageChartOfAccounts("Select");
            HashSet<int> descendants = new HashSet<int>();
            GetDescendants(allAccounts.Select(a => new ChartOfAccountsViewModel { AccountId = a.AccountId, ParentAccountId = a.ParentAccountId }).ToList(), excludeAccountId.Value, descendants);

            parentAccounts = parentAccounts
                .Where(a => a.AccountId != excludeAccountId.Value && !descendants.Contains(a.AccountId))
                .ToList();
        }

        ParentAccountsSelectList = new SelectList(
            parentAccounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text = $"{a.AccountCode} - {a.AccountName}"
            }).ToList(),
            "Value", "Text");
    }

    private void GetDescendants(List<ChartOfAccountsViewModel> allAccounts, int currentAccountId, HashSet<int> descendants)
    {
        var children = allAccounts.Where(a => a.ParentAccountId == currentAccountId).ToList();
        foreach (var child in children)
        {
            if (descendants.Add(child.AccountId))
            {
                GetDescendants(allAccounts, child.AccountId, descendants);
            }
        }
    }
}