using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AccountManagementSystem.DataAccess;
using AccountManagementSystem.Models;
using System.Linq;

namespace AccountManagementSystem.Pages.Vouchers
{
    public class IndexModel : PageModel
    {
        private readonly VoucherDAL _voucherDAL;

        public IndexModel(VoucherDAL voucherDAL)
        {
            _voucherDAL = voucherDAL;
        }

        public List<Voucher> Vouchers { get; set; } = new List<Voucher>();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FilterVoucherId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterVoucherType { get; set; } = "Journal";

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterStartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterEndDate { get; set; }

        public List<string> VoucherTypes { get; set; } = new List<string> { "Journal", "Payment", "Receipt", "All" };

        public IActionResult OnGet()
        {
            try
            {
                string? typeToFetch = FilterVoucherType == "All" ? null : FilterVoucherType;

                Vouchers = _voucherDAL.GetVouchers(
                    voucherId: FilterVoucherId,
                    voucherType: typeToFetch,
                    startDate: FilterStartDate,
                    endDate: FilterEndDate
                );
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading vouchers: {ex.Message}";
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            return OnGet();
        }
    }
}