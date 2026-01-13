using System.ComponentModel.DataAnnotations;
using LoanApplication.API.Models;

namespace LoanApplication.API.DTOs;

/// <summary>
/// DTO for creating a new loan application
/// </summary>
public class CreateLoanDto
{
    [Required(ErrorMessage = "Applicant name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string ApplicantName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string ApplicantEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string ApplicantPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loan amount is required")]
    [Range(1000, 10000000, ErrorMessage = "Loan amount must be between 1,000 and 10,000,000")]
    public decimal LoanAmount { get; set; }

    [Required(ErrorMessage = "Loan term is required")]
    [Range(1, 360, ErrorMessage = "Loan term must be between 1 and 360 months")]
    public int LoanTermMonths { get; set; }

    [Required(ErrorMessage = "Interest rate is required")]
    [Range(0.01, 30.00, ErrorMessage = "Interest rate must be between 0.01% and 30.00%")]
    public decimal InterestRate { get; set; }

    [Required(ErrorMessage = "Loan type is required")]
    public LoanType LoanType { get; set; }

    [Required(ErrorMessage = "Purpose is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Purpose must be between 10 and 500 characters")]
    public string Purpose { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing loan application
/// </summary>
public class UpdateLoanDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string? ApplicantName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? ApplicantEmail { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? ApplicantPhone { get; set; }

    [Range(1000, 10000000, ErrorMessage = "Loan amount must be between 1,000 and 10,000,000")]
    public decimal? LoanAmount { get; set; }

    [Range(1, 360, ErrorMessage = "Loan term must be between 1 and 360 months")]
    public int? LoanTermMonths { get; set; }

    [Range(0.01, 30.00, ErrorMessage = "Interest rate must be between 0.01% and 30.00%")]
    public decimal? InterestRate { get; set; }

    public LoanType? LoanType { get; set; }

    public LoanStatus? Status { get; set; }

    [StringLength(500, MinimumLength = 10, ErrorMessage = "Purpose must be between 10 and 500 characters")]
    public string? Purpose { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for loan response
/// </summary>
public class LoanResponseDto
{
    public int Id { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string ApplicantPhone { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public string LoanType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public decimal? MonthlyPayment { get; set; }
    public string? Notes { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for paginated loan list response
/// </summary>
public class PaginatedLoanResponseDto
{
    public List<LoanResponseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// DTO for loan status update
/// </summary>
public class UpdateLoanStatusDto
{
    [Required(ErrorMessage = "Status is required")]
    public LoanStatus Status { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }
}

/// <summary>
/// Generic API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
