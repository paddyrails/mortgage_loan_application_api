using LoanApplication.API.DTOs;
using LoanApplication.API.Models;

namespace LoanApplication.API.Services;

/// <summary>
/// Interface for loan service operations
/// </summary>
public interface ILoanService
{
    Task<PaginatedLoanResponseDto> GetAllLoansAsync(int pageNumber, int pageSize, LoanStatus? status = null, LoanType? type = null);
    Task<LoanResponseDto?> GetLoanByIdAsync(int id);
    Task<LoanResponseDto?> GetLoanByNumberAsync(string loanNumber);
    Task<LoanResponseDto> CreateLoanAsync(CreateLoanDto createLoanDto);
    Task<LoanResponseDto?> UpdateLoanAsync(int id, UpdateLoanDto updateLoanDto);
    Task<LoanResponseDto?> UpdateLoanStatusAsync(int id, UpdateLoanStatusDto statusDto);
    Task<bool> DeleteLoanAsync(int id);
    Task<IEnumerable<LoanResponseDto>> SearchLoansAsync(string searchTerm);
    Task<LoanStatisticsDto> GetLoanStatisticsAsync();
}

/// <summary>
/// DTO for loan statistics
/// </summary>
public class LoanStatisticsDto
{
    public int TotalLoans { get; set; }
    public int PendingLoans { get; set; }
    public int ApprovedLoans { get; set; }
    public int RejectedLoans { get; set; }
    public int DisbursedLoans { get; set; }
    public decimal TotalLoanAmount { get; set; }
    public decimal TotalDisbursedAmount { get; set; }
    public decimal AverageLoanAmount { get; set; }
    public Dictionary<string, int> LoansByType { get; set; } = new();
    public Dictionary<string, int> LoansByStatus { get; set; } = new();
}
