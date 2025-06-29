using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AccountManagementSystem.Models;

public class ChartOfAccountsViewModel
{
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Account Code is required.")]
    [StringLength(50, ErrorMessage = "Account Code cannot exceed 50 characters.")]
    [Display(Name = "Account Code")]
    public string AccountCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account Name is required.")]
    [StringLength(255, ErrorMessage = "Account Name cannot exceed 255 characters.")]
    [Display(Name = "Account Name")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account Type is required.")]
    [Display(Name = "Account Type")]
    public string AccountType { get; set; } = string.Empty;

    [Display(Name = "Parent Account")]
    public int? ParentAccountId { get; set; }

    public string? ParentAccountName { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Created Date")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public List<ChartOfAccountsViewModel> Children { get; set; } = new List<ChartOfAccountsViewModel>();
    public int Level { get; set; } = 0;
}
