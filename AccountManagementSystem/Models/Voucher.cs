using System.Linq;
namespace AccountManagementSystem.Models
{
    public class Voucher
    {
        public int VoucherId { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public List<VoucherDetail> Details { get; set; } = new List<VoucherDetail>();
        public decimal TotalDebit => Details.Sum(d => d.DebitAmount);
        public decimal TotalCredit => Details.Sum(d => d.CreditAmount);
    }
    public class VoucherDetail
    {
        public int VoucherDetailId { get; set; }
        public int VoucherId { get; set; }
        public int AccountId { get; set; }
        public string AccountCode { get; set; } = string.Empty; 
        public string AccountName { get; set; } = string.Empty;
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string? Narration { get; set; }
    }
}
