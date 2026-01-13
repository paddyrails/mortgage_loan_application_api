using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanApplication.API.Models;

/// <summary>
/// Represents a loan application entity
/// </summary>
public class Loan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string LoanNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string ApplicantEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string ApplicantPhone { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(1000, 10000000, ErrorMessage = "Loan amount must be between 1,000 and 10,000,000")]
    public decimal LoanAmount { get; set; }

    [Required]
    [Range(1, 360, ErrorMessage = "Loan term must be between 1 and 360 months")]
    public int LoanTermMonths { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    [Range(0.01, 30.00, ErrorMessage = "Interest rate must be between 0.01% and 30.00%")]
    public decimal InterestRate { get; set; }

    [Required]
    public LoanType LoanType { get; set; }

    [Required]
    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    [Required]
    [StringLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MonthlyPayment { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public DateTime? ApprovalDate { get; set; }

    public DateTime? DisbursementDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Types of loans available
/// </summary>
public enum LoanType
{
    Personal = 1,
    Home = 2,
    Auto = 3,
    Business = 4,
    Education = 5,
    Medical = 6
}

/// <summary>
/// Status of the loan application
/// </summary>
public enum LoanStatus
{
    Pending = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Disbursed = 5,
    Closed = 6,
    Defaulted = 7
}
