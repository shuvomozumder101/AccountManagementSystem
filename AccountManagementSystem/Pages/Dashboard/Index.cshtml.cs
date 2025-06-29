using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AccountManagementSystem.DataAccess;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.Pages.Dashboard;

[Authorize(Roles = "Admin,Accountant,Viewer")] 
public class IndexModel : PageModel
{
    private readonly VoucherDAL _voucherDAL;
    private readonly ChartOfAccountsDAL _chartOfAccountsDAL;

    public IndexModel(VoucherDAL voucherDAL, ChartOfAccountsDAL chartOfAccountsDAL)
    {
        _voucherDAL = voucherDAL;
        _chartOfAccountsDAL = chartOfAccountsDAL;
    }

    public int TotalVouchers { get; set; }
    public decimal TotalDebitAmount { get; set; }
    public decimal TotalCreditAmount { get; set; }
    public decimal TotalBalance { get; set; }

    public int TotalAccounts { get; set; }
    public Dictionary<string, int> AccountsByType { get; set; } = new Dictionary<string, int>();

    public List<VoucherViewModel> RecentVouchers { get; set; } = new List<VoucherViewModel>();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        try
        {
            var allVouchers = _voucherDAL.GetVouchers(null, null, null, null);
            TotalVouchers = allVouchers.Count;
            TotalDebitAmount = allVouchers.Sum(v => v.TotalDebit);
            TotalCreditAmount = allVouchers.Sum(v => v.TotalCredit);
            TotalBalance = TotalDebitAmount - TotalCreditAmount;

            RecentVouchers = allVouchers
                .OrderByDescending(v => v.VoucherDate)
                .ThenByDescending(v => v.VoucherId)
                .Take(5)
                .Select(v => new VoucherViewModel
                {
                    VoucherId = v.VoucherId,
                    VoucherType = v.VoucherType,
                    VoucherDate = v.VoucherDate,
                    ReferenceNo = v.ReferenceNo,
                    Description = v.Description,
                    CreatedBy = v.CreatedBy
                })
                .ToList();
            var allAccounts = _chartOfAccountsDAL.ManageChartOfAccounts("Select", null, null, null, null, null, null, null);
            TotalAccounts = allAccounts.Count;
            AccountsByType = allAccounts
                .GroupBy(a => a.AccountType)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading dashboard data: {ex.Message}";
        }
    }
}