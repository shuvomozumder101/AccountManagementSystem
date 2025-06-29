using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AccountManagementSystem.DataAccess;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.Pages.Vouchers;

[Authorize(Roles = "Admin,Accountant")]
public class CreatePaymentModel : PageModel
{
    private readonly VoucherDAL _voucherDAL;
    private readonly ChartOfAccountsDAL _chartOfAccountsDAL;

    public CreatePaymentModel(VoucherDAL voucherDAL, ChartOfAccountsDAL chartOfAccountsDAL)
    {
        _voucherDAL = voucherDAL;
        _chartOfAccountsDAL = chartOfAccountsDAL;
    }

    [BindProperty]
    public VoucherViewModel Voucher { get; set; } = new VoucherViewModel();

    public SelectList AccountsSelectList { get; set; } = new SelectList(new List<SelectListItem>());

    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        Voucher.VoucherType = "Payment";
        Voucher.Details.Add(new VoucherDetailViewModel());
        PopulateAccountsDropdown();
        return Page();
    }

    public IActionResult OnPost()
    {
        Voucher.VoucherType = "Payment";

        if (!ModelState.IsValid)
        {
            PopulateAccountsDropdown();
            return Page();
        }

        if (Math.Abs(Voucher.TotalDebit - Voucher.TotalCredit) > 0.001m)
        {
            ModelState.AddModelError(string.Empty, "Total Debit must equal Total Credit for the voucher.");
            PopulateAccountsDropdown();
            return Page();
        }

        if (Voucher.TotalDebit <= 0 || Voucher.TotalCredit <= 0)
        {
            ModelState.AddModelError(string.Empty, "Voucher must have a positive total debit and a positive total credit.");
            PopulateAccountsDropdown();
            return Page();
        }

        Voucher.CreatedBy = User.Identity?.Name ?? "Unknown";

        try
        {
            var dalVoucher = new Models.Voucher
            {
                VoucherType = Voucher.VoucherType,
                VoucherDate = Voucher.VoucherDate,
                ReferenceNo = Voucher.ReferenceNo,
                Description = Voucher.Description,
                CreatedBy = Voucher.CreatedBy,
                Details = Voucher.Details.Select(d => new Models.VoucherDetail
                {
                    AccountId = d.AccountId,
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount,
                    Narration = d.Narration
                }).ToList()
            };

            int newVoucherId = _voucherDAL.SaveVoucher(dalVoucher);
            StatusMessage = $"Payment Voucher '{Voucher.ReferenceNo}' created successfully with ID: {newVoucherId}.";
            return RedirectToPage("./Index");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = $"Error saving voucher: {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }

        PopulateAccountsDropdown();
        return Page();
    }

    private void PopulateAccountsDropdown()
    {
        var accounts = _chartOfAccountsDAL.GetAccountsForDropdown();
        AccountsSelectList = new SelectList(
            accounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text = $"{a.AccountCode} - {a.AccountName}"
            }).ToList(),
            "Value", "Text");
    }
}