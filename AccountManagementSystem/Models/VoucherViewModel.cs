using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AccountManagementSystem.Models;

public class VoucherViewModel
{
    public int VoucherId { get; set; }
    public string VoucherType { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; } = DateTime.Today;
    public string ReferenceNo { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatedBy { get; set; } = string.Empty; 
    public DateTime CreatedDate { get; set; } = DateTime.Now; 
    public List<VoucherDetailViewModel> Details { get; set; } = new List<VoucherDetailViewModel>();
    public decimal TotalDebit => Details.Sum(d => d.DebitAmount);
    public decimal TotalCredit => Details.Sum(d => d.CreditAmount);
    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }
}

public class VoucherDetailViewModel
{
    public int VoucherDetailId { get; set; }
    public int VoucherId { get; set; }
    public int AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; } 
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? Narration { get; set; }
}