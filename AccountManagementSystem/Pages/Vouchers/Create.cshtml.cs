using AccountManagementSystem.Data;
using AccountManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;

namespace AccountManagementSystem.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
    public class CreateModel(DbContext context, UserManager<ApplicationUser> userManager) : PageModel
    {
        private readonly DbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [BindProperty]
        public required VoucherHeader VoucherHeader { get; set; }

        [BindProperty]
        public List<VoucherDetail> Details { get; set; } = new List<VoucherDetail>();

        public List<Account> Accounts { get; set; } = new List<Account>();

        public async Task OnGetAsync()
        {
            await LoadAccounts();
         
            Details.Add(new VoucherDetail());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAccounts();
                return Page();
            }

            Details = Details
                .Where(d => d.AccountId > 0 && d.Amount > 0)
                .ToList();

           
            decimal totalDebit = Details.Where(d => d.IsDebit).Sum(d => d.Amount);
            decimal totalCredit = Details.Where(d => !d.IsDebit).Sum(d => d.Amount);

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError("", "Total debit and credit amounts must be equal.");
                await LoadAccounts();
                return Page();
            }

                      var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
                        
            var detailsTable = new DataTable();
            detailsTable.Columns.Add("AccountId", typeof(int));
            detailsTable.Columns.Add("Amount", typeof(decimal));
            detailsTable.Columns.Add("IsDebit", typeof(bool));
            detailsTable.Columns.Add("Narration", typeof(string));

            foreach (var detail in Details)
            {
                detailsTable.Rows.Add(detail.AccountId, detail.Amount, detail.IsDebit, detail.Narration);
            }

            using (var connection = _context.CreateConnection())
            {
                var command = new SqlCommand("sp_SaveVoucher", (SqlConnection)connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@VoucherType", VoucherHeader.VoucherType);
                command.Parameters.AddWithValue("@VoucherDate", VoucherHeader.VoucherDate);
                command.Parameters.AddWithValue("@ReferenceNo", VoucherHeader.ReferenceNo);
                command.Parameters.AddWithValue("@Narration", VoucherHeader.Narration);
                command.Parameters.AddWithValue("@CreatedBy", user.Id);

                var param = command.Parameters.AddWithValue("@Details", detailsTable);
                param.SqlDbType = SqlDbType.Structured;
                param.TypeName = "dbo.VoucherDetailType";

                connection.Open();
                await command.ExecuteNonQueryAsync();
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadAccounts()
        {
            using (var connection = _context.CreateConnection())
            {
                var command = new SqlCommand("sp_ManageChartOfAccounts", (SqlConnection)connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Action", "SELECTALL");

                connection.Open();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Accounts.Add(new Account
                    {
                        AccountId = reader.GetInt32("AccountId"),
                        AccountCode = reader.GetString("AccountCode"),
                        AccountName = reader.GetString("AccountName")
                    });
                }
            }
        }
    }

    public class VoucherHeader
    {
        [Required]
        public string VoucherType { get; set; }

        [Required]
        public DateTime VoucherDate { get; set; }

        [Required]
        public string ReferenceNo { get; set; }

        public string Narration { get; set; }
    }

    public class VoucherDetail
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public bool IsDebit { get; set; }

        public string Narration { get; set; }
    }

    public class Account
    {
        public int AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
    }
}