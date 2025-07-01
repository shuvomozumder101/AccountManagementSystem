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
public class CreateReceiptModel : PageModel
{
    private readonly VoucherDAL _voucherDAL;
    private readonly ChartOfAccountsDAL _chartOfAccountsDAL;

    public CreateReceiptModel(VoucherDAL voucherDAL, ChartOfAccountsDAL chartOfAccountsDAL)
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
        Voucher.VoucherType = "Receipt";

        if (!Voucher.Details.Any())
            Voucher.Details.Add(new VoucherDetailViewModel());

        LoadAccountsDropdown();
        return Page();
    }

    public IActionResult OnPost()
    {
        Voucher.VoucherType = "Receipt";
        Voucher.CreatedBy = User.Identity?.Name ?? "System";
        Voucher.CreatedDate = DateTime.Now;

        try
        {
            var newVoucher = MapToEntity(Voucher);
            int voucherId = _voucherDAL.SaveVoucher(newVoucher);
            StatusMessage = $"Receipt Voucher '{Voucher.ReferenceNo}' created successfully with ID: {voucherId}.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while saving the voucher: {ex.Message}";
            LoadAccountsDropdown();
            return Page();
        }
    }

    private void LoadAccountsDropdown()
    {
        var accounts = _chartOfAccountsDAL.GetAccountsForDropdown();
        AccountsSelectList = new SelectList(
            accounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text = $"{a.AccountCode} - {a.AccountName}"
            }),
            "Value", "Text");
    }

    private Voucher MapToEntity(VoucherViewModel model)
    {
        return new Voucher
        {
            VoucherType = model.VoucherType,
            VoucherDate = model.VoucherDate,
            ReferenceNo = model.ReferenceNo,
            Description = model.Description,
            CreatedBy = model.CreatedBy,
            CreatedDate = model.CreatedDate,
            Details = model.Details.Select(d => new AccountManagementSystem.Models.VoucherDetail
            {
                AccountId = d.AccountId,
                DebitAmount = d.DebitAmount,
                CreditAmount = d.CreditAmount,
                Narration = d.Narration ?? string.Empty
            }).ToList()
        };
    }
}