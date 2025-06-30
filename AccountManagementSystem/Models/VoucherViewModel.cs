using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AccountManagementSystem.Models;

public class VoucherViewModel
{
    public int VoucherId { get; set; }

    [Required(ErrorMessage = "Voucher Type is required.")]
    [Display(Name = "Voucher Type")]
    public string VoucherType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Voucher Date is required.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Voucher Date")]
    public DateTime VoucherDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Reference No. is required.")]
    [StringLength(100, ErrorMessage = "Reference No. cannot exceed 100 characters.")]
    [Display(Name = "Reference No.")]
    public string ReferenceNo { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }

    public string CreatedBy { get; set; } = string.Empty; 
    public DateTime CreatedDate { get; set; } = DateTime.Now; 
    public List<VoucherDetailViewModel> Details { get; set; } = new List<VoucherDetailViewModel>();

    // Calculated totals from detail view models for display and validation
    public decimal TotalDebit => Details.Sum(d => d.DebitAmount);
    public decimal TotalCredit => Details.Sum(d => d.CreditAmount);
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }
}

public class VoucherDetailViewModel
{
    public int VoucherDetailId { get; set; }
    public int VoucherId { get; set; }

    [Required(ErrorMessage = "Account is required for voucher detail.")]
    [Display(Name = "Account")]
    public int AccountId { get; set; }

    public string? AccountCode { get; set; }
    public string? AccountName { get; set; } 

    [Display(Name = "Debit")]
    [Range(0, 999999999999999.99, ErrorMessage = "Debit amount must be non-negative.")]
    [DataType(DataType.Currency)]
    public decimal DebitAmount { get; set; }

    [Display(Name = "Credit")]
    [Range(0, 999999999999999.99, ErrorMessage = "Credit amount must be non-negative.")]
    [DataType(DataType.Currency)] 
    public decimal CreditAmount { get; set; }

    [StringLength(500, ErrorMessage = "Narration cannot exceed 500 characters.")]
    [Display(Name = "Narration")]
    public string? Narration { get; set; }
}