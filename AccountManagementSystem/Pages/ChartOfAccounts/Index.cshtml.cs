using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AccountManagementSystem.DataAccess;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.Pages.ChartOfAccounts;

[Authorize(Roles = "Admin,Accountant,Viewer")]
public class IndexModel : PageModel
{
    private readonly ChartOfAccountsDAL _chartOfAccounts;

    public IndexModel(ChartOfAccountsDAL chartOfAccounts)
    {
        _chartOfAccounts = chartOfAccounts;
    }

    public List<ChartOfAccountsViewModel> Accounts { get; set; } = new List<ChartOfAccountsViewModel>();

    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        LoadAccounts();
    }

    public IActionResult OnPostDelete(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            ErrorMessage = "You do not have permission to delete accounts.";
            LoadAccounts();
            return Page();
        }

        try
        {
            _chartOfAccounts.ManageChartOfAccounts("Delete", id);
            StatusMessage = "Account deleted successfully.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting account: {ex.Message}";
        }

        LoadAccounts();
        return Page();
    }

    private void LoadAccounts()
    {
        var allAccountsFromDb = _chartOfAccounts.ManageChartOfAccounts("Select");

        var allAccounts = allAccountsFromDb.Select(a => new ChartOfAccountsViewModel
        {
            AccountId = a.AccountId,
            AccountCode = a.AccountCode,
            AccountName = a.AccountName,
            AccountType = a.AccountType,
            ParentAccountId = a.ParentAccountId,
            IsActive = a.IsActive,
            CreatedDate = a.CreatedDate
        }).ToList();


        foreach (var account in allAccounts)
        {
            if (account.ParentAccountId.HasValue)
            {
                account.ParentAccountName = allAccounts.FirstOrDefault(p => p.AccountId == account.ParentAccountId)?.AccountName;
            }
        }

        Accounts = BuildAccountHierarchy(allAccounts, null);
    }

    private List<ChartOfAccountsViewModel> BuildAccountHierarchy(List<ChartOfAccountsViewModel> allAccounts, int? parentId, int level = 0)
    {
        var topLevelAccounts = allAccounts
            .Where(a => a.ParentAccountId == parentId)
            .ToList();

        foreach (var account in topLevelAccounts)
        {
            account.Level = level;
            account.Children = BuildAccountHierarchy(allAccounts, account.AccountId, level + 1);
        }

        return topLevelAccounts;
    }
}