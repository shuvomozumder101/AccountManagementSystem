using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
public class CreateJournalModel : PageModel
{
    private readonly VoucherDAL _voucherDAL;
    private readonly ChartOfAccountsDAL _chartOfAccountsDAL;

    public CreateJournalModel(VoucherDAL voucherDAL, ChartOfAccountsDAL chartOfAccountsDAL)
    {
        _voucherDAL = voucherDAL;
        _chartOfAccountsDAL = chartOfAccountsDAL;
    }

    [BindProperty]
    public VoucherViewModel Voucher { get; set; } = new VoucherViewModel();

    public SelectList AccountsSelectList { get; set; } = new SelectList(new List<SelectListItem>());

    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        Voucher.VoucherType = "Journal";
        if (!Voucher.Details.Any())
        {
            Voucher.Details.Add(new VoucherDetailViewModel());
        }

        PopulateAccountsDropdown();
        return Page();
    }

    public IActionResult OnPost()
    {
        Voucher.VoucherType = "Journal";

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

        foreach (var detail in Voucher.Details)
        {
            if (detail.DebitAmount > 0 && detail.CreditAmount > 0)
            {
                ModelState.AddModelError(string.Empty, "A voucher detail row cannot have both a Debit and a Credit amount greater than zero.");
                PopulateAccountsDropdown();
                return Page();
            }
            if (detail.DebitAmount == 0 && detail.CreditAmount == 0)
            {
                ModelState.AddModelError(string.Empty, "A voucher detail row must have either a Debit or a Credit amount.");
                PopulateAccountsDropdown();
                return Page();
            }
        }

        Voucher.CreatedBy = User.Identity?.Name ?? "Unknown";
        Voucher.CreatedDate = DateTime.Now;

        try
        {
            var dalVoucher = new Models.Voucher
            {
                VoucherType = Voucher.VoucherType,
                VoucherDate = Voucher.VoucherDate,
                ReferenceNo = Voucher.ReferenceNo,
                Description = Voucher.Description,
                CreatedBy = Voucher.CreatedBy,
                CreatedDate = Voucher.CreatedDate,
                Details = Voucher.Details.Select(d => new Models.VoucherDetail
                {
                    AccountId = d.AccountId,
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount,
                    Narration = d.Narration
                }).ToList()
            };

            int newVoucherId = _voucherDAL.SaveVoucher(dalVoucher);

            StatusMessage = $"Journal Voucher '{Voucher.ReferenceNo}' created successfully with ID: {newVoucherId}.";
            return RedirectToPage("./Index");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = $"Error saving voucher (Invalid Operation): {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }

        PopulateAccountsDropdown();
        return Page();
    }
    public void PopulateAccountsDropdown()
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