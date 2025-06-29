using AccountManagementSystem.DataAccess;
using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccountManagementSystem.Pages.Vouchers;

[Authorize(Roles = "Admin,Accountant,Viewer")]
public class IndexModel : PageModel
{
    private readonly VoucherDAL _voucherDAL;

    public IndexModel(VoucherDAL voucherDAL)
    {
        _voucherDAL = voucherDAL;
    }

    public List<VoucherViewModel> Vouchers { get; set; } = new List<VoucherViewModel>();

    [BindProperty(SupportsGet = true)]
    public string? FilterVoucherType { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }
    [TempData]
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        LoadVouchers();
    }

    public void OnGetFilter()
    {
        LoadVouchers();
    }
    public JsonResult OnGetVoucherDetails(int id)
    {
        try
        {
            var voucher = _voucherDAL.GetVouchers(id, null, null, null).FirstOrDefault();
            if (voucher == null)
            {
                return new JsonResult(new { success = false, message = "Voucher not found." }) { StatusCode = 404 };
            }
            var voucherViewModel = new VoucherViewModel
            {
                VoucherId = voucher.VoucherId,
                VoucherType = voucher.VoucherType,
                VoucherDate = voucher.VoucherDate,
                ReferenceNo = voucher.ReferenceNo,
                Description = voucher.Description,
                CreatedBy = voucher.CreatedBy,
                CreatedDate = voucher.CreatedDate,
                Details = voucher.Details.Select(d => new VoucherDetailViewModel
                {
                    VoucherDetailId = d.VoucherDetailId,
                    AccountId = d.AccountId,
                    AccountCode = d.AccountCode,
                    AccountName = d.AccountName,
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount,
                    Narration = d.Narration
                }).ToList()
            };

            return new JsonResult(voucherViewModel);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error loading voucher details: {ex.Message}" }) { StatusCode = 500 };
        }
    }

    private void LoadVouchers()
    {
        try
        {
            Vouchers = _voucherDAL.GetVouchers(
                voucherId: null,
                voucherType: FilterVoucherType,
                startDate: StartDate,
                endDate: EndDate
            ).Select(v => new VoucherViewModel
            {
                VoucherId = v.VoucherId,
                VoucherType = v.VoucherType,
                VoucherDate = v.VoucherDate,
                ReferenceNo = v.ReferenceNo,
                Description = v.Description,
                CreatedBy = v.CreatedBy,
                CreatedDate = v.CreatedDate,
                Details = v.Details.Select(d => new VoucherDetailViewModel
                {
                    AccountId = d.AccountId,
                    AccountCode = d.AccountCode,
                    AccountName = d.AccountName,
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount,
                    Narration = d.Narration
                }).ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading vouchers: {ex.Message}";
        }
    }
}
