using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using LoanApplication.API.DTOs;
using LoanApplication.API.Models;
using LoanApplication.API.Services;

namespace LoanApplication.API.Controllers;

/// <summary>
/// API Controller for Loan CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ILogger<LoansController> _logger;

    public LoansController(ILoanService loanService, ILogger<LoansController> logger)
    {
        _loanService = loanService;
        _logger = logger;
    }

    /// <summary>
    /// Get all loans with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="status">Filter by loan status</param>
    /// <param name="type">Filter by loan type</param>
    /// <returns>Paginated list of loans</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all loans", Description = "Retrieves a paginated list of all loan applications")]
    [SwaggerResponse(200, "Successfully retrieved loans", typeof(ApiResponse<PaginatedLoanResponseDto>))]
    public async Task<ActionResult<ApiResponse<PaginatedLoanResponseDto>>> GetAllLoans(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] LoanStatus? status = null,
        [FromQuery] LoanType? type = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var result = await _loanService.GetAllLoansAsync(pageNumber, pageSize, status, type);
        return Ok(ApiResponse<PaginatedLoanResponseDto>.SuccessResponse(result, "Loans retrieved successfully"));
    }

    /// <summary>
    /// Get a loan by ID
    /// </summary>
    /// <param name="id">Loan ID</param>
    /// <returns>Loan details</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get loan by ID", Description = "Retrieves a specific loan application by its ID")]
    [SwaggerResponse(200, "Successfully retrieved loan", typeof(ApiResponse<LoanResponseDto>))]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<ApiResponse<LoanResponseDto>>> GetLoanById(int id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);
        
        if (loan == null)
        {
            return NotFound(ApiResponse<LoanResponseDto>.FailResponse($"Loan with ID {id} not found"));
        }

        return Ok(ApiResponse<LoanResponseDto>.SuccessResponse(loan, "Loan retrieved successfully"));
    }

    /// <summary>
    /// Get a loan by loan number
    /// </summary>
    /// <param name="loanNumber">Loan number (e.g., LN-2024-000001)</param>
    /// <returns>Loan details</returns>
    [HttpGet("number/{loanNumber}")]
    [SwaggerOperation(Summary = "Get loan by number", Description = "Retrieves a specific loan application by its loan number")]
    [SwaggerResponse(200, "Successfully retrieved loan", typeof(ApiResponse<LoanResponseDto>))]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<ApiResponse<LoanResponseDto>>> GetLoanByNumber(string loanNumber)
    {
        var loan = await _loanService.GetLoanByNumberAsync(loanNumber);
        
        if (loan == null)
        {
            return NotFound(ApiResponse<LoanResponseDto>.FailResponse($"Loan with number {loanNumber} not found"));
        }

        return Ok(ApiResponse<LoanResponseDto>.SuccessResponse(loan, "Loan retrieved successfully"));
    }

    /// <summary>
    /// Create a new loan application
    /// </summary>
    /// <param name="createLoanDto">Loan creation data</param>
    /// <returns>Created loan details</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a new loan", Description = "Creates a new loan application")]
    [SwaggerResponse(201, "Loan created successfully", typeof(ApiResponse<LoanResponseDto>))]
    [SwaggerResponse(400, "Invalid input data")]
    public async Task<ActionResult<ApiResponse<LoanResponseDto>>> CreateLoan([FromBody] CreateLoanDto createLoanDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<LoanResponseDto>.FailResponse("Validation failed", errors));
        }

        try
        {
            var loan = await _loanService.CreateLoanAsync(createLoanDto);
            _logger.LogInformation("New loan created: {LoanNumber}", loan.LoanNumber);
            
            return CreatedAtAction(
                nameof(GetLoanById), 
                new { id = loan.Id }, 
                ApiResponse<LoanResponseDto>.SuccessResponse(loan, "Loan created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating loan");
            return BadRequest(ApiResponse<LoanResponseDto>.FailResponse("Failed to create loan", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update an existing loan application
    /// </summary>
    /// <param name="id">Loan ID</param>
    /// <param name="updateLoanDto">Loan update data</param>
    /// <returns>Updated loan details</returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a loan", Description = "Updates an existing loan application")]
    [SwaggerResponse(200, "Loan updated successfully", typeof(ApiResponse<LoanResponseDto>))]
    [SwaggerResponse(400, "Invalid input data")]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<ApiResponse<LoanResponseDto>>> UpdateLoan(int id, [FromBody] UpdateLoanDto updateLoanDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<LoanResponseDto>.FailResponse("Validation failed", errors));
        }

        try
        {
            var loan = await _loanService.UpdateLoanAsync(id, updateLoanDto);
            
            if (loan == null)
            {
                return NotFound(ApiResponse<LoanResponseDto>.FailResponse($"Loan with ID {id} not found"));
            }

            _logger.LogInformation("Loan updated: {LoanNumber}", loan.LoanNumber);
            return Ok(ApiResponse<LoanResponseDto>.SuccessResponse(loan, "Loan updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loan {Id}", id);
            return BadRequest(ApiResponse<LoanResponseDto>.FailResponse("Failed to update loan", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update loan status
    /// </summary>
    /// <param name="id">Loan ID</param>
    /// <param name="statusDto">New status data</param>
    /// <returns>Updated loan details</returns>
    [HttpPatch("{id:int}/status")]
    [SwaggerOperation(Summary = "Update loan status", Description = "Updates the status of an existing loan application")]
    [SwaggerResponse(200, "Loan status updated successfully", typeof(ApiResponse<LoanResponseDto>))]
    [SwaggerResponse(400, "Invalid input data")]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<ApiResponse<LoanResponseDto>>> UpdateLoanStatus(int id, [FromBody] UpdateLoanStatusDto statusDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<LoanResponseDto>.FailResponse("Validation failed", errors));
        }

        try
        {
            var loan = await _loanService.UpdateLoanStatusAsync(id, statusDto);
            
            if (loan == null)
            {
                return NotFound(ApiResponse<LoanResponseDto>.FailResponse($"Loan with ID {id} not found"));
            }

            _logger.LogInformation("Loan status updated: {LoanNumber} to {Status}", loan.LoanNumber, loan.Status);
            return Ok(ApiResponse<LoanResponseDto>.SuccessResponse(loan, "Loan status updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loan status {Id}", id);
            return BadRequest(ApiResponse<LoanResponseDto>.FailResponse("Failed to update loan status", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a loan application
    /// </summary>
    /// <param name="id">Loan ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Delete a loan", Description = "Deletes an existing loan application")]
    [SwaggerResponse(200, "Loan deleted successfully")]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteLoan(int id)
    {
        try
        {
            var result = await _loanService.DeleteLoanAsync(id);
            
            if (!result)
            {
                return NotFound(ApiResponse<object>.FailResponse($"Loan with ID {id} not found"));
            }

            _logger.LogInformation("Loan deleted: {Id}", id);
            return Ok(ApiResponse<object>.SuccessResponse(new { Id = id }, "Loan deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting loan {Id}", id);
            return BadRequest(ApiResponse<object>.FailResponse("Failed to delete loan", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Search loans
    /// </summary>
    /// <param name="searchTerm">Search term (name, email, or loan number)</param>
    /// <returns>List of matching loans</returns>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search loans", Description = "Search loans by applicant name, email, or loan number")]
    [SwaggerResponse(200, "Search completed", typeof(ApiResponse<IEnumerable<LoanResponseDto>>))]
    public async Task<ActionResult<ApiResponse<IEnumerable<LoanResponseDto>>>> SearchLoans([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            return BadRequest(ApiResponse<IEnumerable<LoanResponseDto>>.FailResponse("Search term must be at least 2 characters"));
        }

        var loans = await _loanService.SearchLoansAsync(searchTerm);
        return Ok(ApiResponse<IEnumerable<LoanResponseDto>>.SuccessResponse(loans, $"Found {loans.Count()} loan(s)"));
    }

    /// <summary>
    /// Get loan statistics
    /// </summary>
    /// <returns>Loan statistics</returns>
    [HttpGet("statistics")]
    [SwaggerOperation(Summary = "Get loan statistics", Description = "Retrieves overall loan statistics")]
    [SwaggerResponse(200, "Statistics retrieved", typeof(ApiResponse<LoanStatisticsDto>))]
    public async Task<ActionResult<ApiResponse<LoanStatisticsDto>>> GetStatistics()
    {
        var stats = await _loanService.GetLoanStatisticsAsync();
        return Ok(ApiResponse<LoanStatisticsDto>.SuccessResponse(stats, "Statistics retrieved successfully"));
    }
}
