using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AccountManagementSystem.DataAccess;
using AccountManagementSystem.Models;

namespace AccountManagementSystem.Pages.Vouchers
{
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
        public VoucherViewModel Voucher { get; set; } = new();

        public SelectList AccountsSelectList { get; set; } = default!;

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            Voucher.VoucherType = "Journal";

            if (!Voucher.Details.Any())
                Voucher.Details.Add(new VoucherDetailViewModel());

            LoadAccountsDropdown();
            return Page();
        }

        public IActionResult OnPost()
        {
            Voucher.VoucherType = "Journal";

            if (!ModelState.IsValid)
            {
                LoadAccountsDropdown();
                return Page();
            }

            if (!IsValidVoucher(Voucher, out string validationError))
            {
                ModelState.AddModelError(string.Empty, validationError);
                LoadAccountsDropdown();
                return Page();
            }

            Voucher.CreatedBy = User.Identity?.Name ?? "System";
            Voucher.CreatedDate = DateTime.Now;

            try
            {
                var newVoucher = MapToEntity(Voucher);
                int voucherId = _voucherDAL.SaveVoucher(newVoucher);

                StatusMessage = $"Journal Voucher '{Voucher.ReferenceNo}' created successfully with ID: {voucherId}.";
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
            AccountsSelectList = new SelectList(accounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text = $"{a.AccountCode} - {a.AccountName}"
            }), "Value", "Text");
        }

        private bool IsValidVoucher(VoucherViewModel voucher, out string error)
        {
            error = string.Empty;

            if (voucher.TotalDebit <= 0 || voucher.TotalCredit <= 0)
            {
                error = "Voucher must have a positive total debit and credit.";
                return false;
            }

            if (Math.Abs(voucher.TotalDebit - voucher.TotalCredit) > 0.001m)
            {
                error = "Total Debit must equal Total Credit.";
                return false;
            }

            foreach (var detail in voucher.Details)
            {
                if (detail.DebitAmount > 0 && detail.CreditAmount > 0)
                {
                    error = "A detail row cannot have both debit and credit amounts.";
                    return false;
                }

                if (detail.DebitAmount == 0 && detail.CreditAmount == 0)
                {
                    error = "Each detail row must have either a debit or credit amount.";
                    return false;
                }
            }

            return true;
        }

        private Voucher MapToEntity(VoucherViewModel Model)
        {
            return new Voucher
            {
                VoucherType = Model.VoucherType,
                VoucherDate = Model.VoucherDate,
                ReferenceNo = Model.ReferenceNo,
                Description = Model.Description,
                CreatedBy = Model.CreatedBy,
                CreatedDate = Model.CreatedDate,
                Details = Model.Details.Select(d => new AccountManagementSystem.Models.VoucherDetail
                {
                    AccountId = d.AccountId,
                    DebitAmount = d.DebitAmount,
                    CreditAmount = d.CreditAmount,
                    Narration = d.Narration ?? string.Empty
                }).ToList()
            };
        }
    }
}