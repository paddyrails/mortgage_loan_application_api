using Microsoft.EntityFrameworkCore;
using LoanApplication.API.Data;
using LoanApplication.API.DTOs;
using LoanApplication.API.Models;

namespace LoanApplication.API.Services;

/// <summary>
/// Implementation of loan service operations
/// </summary>
public class LoanService : ILoanService
{
    private readonly LoanDbContext _context;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LoanDbContext context, ILogger<LoanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all loans with pagination and optional filtering
    /// </summary>
    public async Task<PaginatedLoanResponseDto> GetAllLoansAsync(
        int pageNumber, 
        int pageSize, 
        LoanStatus? status = null, 
        LoanType? type = null)
    {
        var query = _context.Loans.AsQueryable();

        // Apply filters
        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(l => l.LoanType == type.Value);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var loans = await query
            .OrderByDescending(l => l.ApplicationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedLoanResponseDto
        {
            Items = loans.Select(MapToResponseDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }

    /// <summary>
    /// Get a loan by its ID
    /// </summary>
    public async Task<LoanResponseDto?> GetLoanByIdAsync(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        return loan == null ? null : MapToResponseDto(loan);
    }

    /// <summary>
    /// Get a loan by its loan number
    /// </summary>
    public async Task<LoanResponseDto?> GetLoanByNumberAsync(string loanNumber)
    {
        var loan = await _context.Loans
            .FirstOrDefaultAsync(l => l.LoanNumber == loanNumber);
        return loan == null ? null : MapToResponseDto(loan);
    }

    /// <summary>
    /// Create a new loan application
    /// </summary>
    public async Task<LoanResponseDto> CreateLoanAsync(CreateLoanDto createLoanDto)
    {
        var loan = new Loan
        {
            LoanNumber = await GenerateLoanNumberAsync(),
            ApplicantName = createLoanDto.ApplicantName,
            ApplicantEmail = createLoanDto.ApplicantEmail,
            ApplicantPhone = createLoanDto.ApplicantPhone,
            LoanAmount = createLoanDto.LoanAmount,
            LoanTermMonths = createLoanDto.LoanTermMonths,
            InterestRate = createLoanDto.InterestRate,
            LoanType = createLoanDto.LoanType,
            Status = LoanStatus.Pending,
            Purpose = createLoanDto.Purpose,
            Notes = createLoanDto.Notes,
            MonthlyPayment = CalculateMonthlyPayment(
                createLoanDto.LoanAmount, 
                createLoanDto.InterestRate, 
                createLoanDto.LoanTermMonths),
            ApplicationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "API"
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new loan application: {LoanNumber}", loan.LoanNumber);

        return MapToResponseDto(loan);
    }

    /// <summary>
    /// Update an existing loan application
    /// </summary>
    public async Task<LoanResponseDto?> UpdateLoanAsync(int id, UpdateLoanDto updateLoanDto)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
        {
            return null;
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(updateLoanDto.ApplicantName))
            loan.ApplicantName = updateLoanDto.ApplicantName;

        if (!string.IsNullOrWhiteSpace(updateLoanDto.ApplicantEmail))
            loan.ApplicantEmail = updateLoanDto.ApplicantEmail;

        if (!string.IsNullOrWhiteSpace(updateLoanDto.ApplicantPhone))
            loan.ApplicantPhone = updateLoanDto.ApplicantPhone;

        if (updateLoanDto.LoanAmount.HasValue)
            loan.LoanAmount = updateLoanDto.LoanAmount.Value;

        if (updateLoanDto.LoanTermMonths.HasValue)
            loan.LoanTermMonths = updateLoanDto.LoanTermMonths.Value;

        if (updateLoanDto.InterestRate.HasValue)
            loan.InterestRate = updateLoanDto.InterestRate.Value;

        if (updateLoanDto.LoanType.HasValue)
            loan.LoanType = updateLoanDto.LoanType.Value;

        if (updateLoanDto.Status.HasValue)
            loan.Status = updateLoanDto.Status.Value;

        if (!string.IsNullOrWhiteSpace(updateLoanDto.Purpose))
            loan.Purpose = updateLoanDto.Purpose;

        if (updateLoanDto.Notes != null)
            loan.Notes = updateLoanDto.Notes;

        // Recalculate monthly payment if relevant fields changed
        if (updateLoanDto.LoanAmount.HasValue || updateLoanDto.InterestRate.HasValue || updateLoanDto.LoanTermMonths.HasValue)
        {
            loan.MonthlyPayment = CalculateMonthlyPayment(
                loan.LoanAmount, 
                loan.InterestRate, 
                loan.LoanTermMonths);
        }

        loan.UpdatedAt = DateTime.UtcNow;
        loan.UpdatedBy = "API";

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated loan application: {LoanNumber}", loan.LoanNumber);

        return MapToResponseDto(loan);
    }

    /// <summary>
    /// Update loan status
    /// </summary>
    public async Task<LoanResponseDto?> UpdateLoanStatusAsync(int id, UpdateLoanStatusDto statusDto)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
        {
            return null;
        }

        var previousStatus = loan.Status;
        loan.Status = statusDto.Status;
        loan.UpdatedAt = DateTime.UtcNow;
        loan.UpdatedBy = "API";

        // Set approval/disbursement dates based on status
        if (statusDto.Status == LoanStatus.Approved && !loan.ApprovalDate.HasValue)
        {
            loan.ApprovalDate = DateTime.UtcNow;
        }
        else if (statusDto.Status == LoanStatus.Disbursed && !loan.DisbursementDate.HasValue)
        {
            loan.DisbursementDate = DateTime.UtcNow;
        }

        if (!string.IsNullOrWhiteSpace(statusDto.Remarks))
        {
            loan.Notes = string.IsNullOrWhiteSpace(loan.Notes) 
                ? statusDto.Remarks 
                : $"{loan.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Status changed from {previousStatus} to {statusDto.Status}: {statusDto.Remarks}";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated loan status: {LoanNumber} from {PreviousStatus} to {NewStatus}", 
            loan.LoanNumber, previousStatus, statusDto.Status);

        return MapToResponseDto(loan);
    }

    /// <summary>
    /// Delete a loan application
    /// </summary>
    public async Task<bool> DeleteLoanAsync(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan == null)
        {
            return false;
        }

        _context.Loans.Remove(loan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted loan application: {LoanNumber}", loan.LoanNumber);

        return true;
    }

    /// <summary>
    /// Search loans by applicant name, email, or loan number
    /// </summary>
    public async Task<IEnumerable<LoanResponseDto>> SearchLoansAsync(string searchTerm)
    {
        var searchTermLower = searchTerm.ToLower();

        var loans = await _context.Loans
            .Where(l => l.ApplicantName.ToLower().Contains(searchTermLower) ||
                       l.ApplicantEmail.ToLower().Contains(searchTermLower) ||
                       l.LoanNumber.ToLower().Contains(searchTermLower))
            .OrderByDescending(l => l.ApplicationDate)
            .Take(50)
            .ToListAsync();

        return loans.Select(MapToResponseDto);
    }

    /// <summary>
    /// Get loan statistics
    /// </summary>
    public async Task<LoanStatisticsDto> GetLoanStatisticsAsync()
    {
        var loans = await _context.Loans.ToListAsync();

        var stats = new LoanStatisticsDto
        {
            TotalLoans = loans.Count,
            PendingLoans = loans.Count(l => l.Status == LoanStatus.Pending),
            ApprovedLoans = loans.Count(l => l.Status == LoanStatus.Approved),
            RejectedLoans = loans.Count(l => l.Status == LoanStatus.Rejected),
            DisbursedLoans = loans.Count(l => l.Status == LoanStatus.Disbursed),
            TotalLoanAmount = loans.Sum(l => l.LoanAmount),
            TotalDisbursedAmount = loans.Where(l => l.Status == LoanStatus.Disbursed).Sum(l => l.LoanAmount),
            AverageLoanAmount = loans.Any() ? loans.Average(l => l.LoanAmount) : 0,
            LoansByType = loans.GroupBy(l => l.LoanType.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            LoansByStatus = loans.GroupBy(l => l.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    #region Private Helper Methods

    /// <summary>
    /// Generate a unique loan number
    /// </summary>
    private async Task<string> GenerateLoanNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var lastLoan = await _context.Loans
            .Where(l => l.LoanNumber.StartsWith($"LN-{year}"))
            .OrderByDescending(l => l.LoanNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastLoan != null)
        {
            var parts = lastLoan.LoanNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"LN-{year}-{nextNumber:D6}";
    }

    /// <summary>
    /// Calculate monthly payment using standard amortization formula
    /// </summary>
    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int termMonths)
    {
        if (termMonths <= 0 || principal <= 0)
            return 0;

        if (annualRate <= 0)
            return principal / termMonths;

        var monthlyRate = annualRate / 100 / 12;
        var payment = principal * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), termMonths)) 
                      / ((decimal)Math.Pow((double)(1 + monthlyRate), termMonths) - 1);

        return Math.Round(payment, 2);
    }

    /// <summary>
    /// Map Loan entity to LoanResponseDto
    /// </summary>
    private static LoanResponseDto MapToResponseDto(Loan loan)
    {
        return new LoanResponseDto
        {
            Id = loan.Id,
            LoanNumber = loan.LoanNumber,
            ApplicantName = loan.ApplicantName,
            ApplicantEmail = loan.ApplicantEmail,
            ApplicantPhone = loan.ApplicantPhone,
            LoanAmount = loan.LoanAmount,
            LoanTermMonths = loan.LoanTermMonths,
            InterestRate = loan.InterestRate,
            LoanType = loan.LoanType.ToString(),
            Status = loan.Status.ToString(),
            Purpose = loan.Purpose,
            MonthlyPayment = loan.MonthlyPayment,
            Notes = loan.Notes,
            ApplicationDate = loan.ApplicationDate,
            ApprovalDate = loan.ApprovalDate,
            DisbursementDate = loan.DisbursementDate,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };
    }

    #endregion
}
