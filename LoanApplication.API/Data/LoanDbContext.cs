using Microsoft.EntityFrameworkCore;
using LoanApplication.API.Models;

namespace LoanApplication.API.Data;

/// <summary>
/// Database context for the Loan Application
/// </summary>
public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
    }

    public DbSet<Loan> Loans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Loan entity
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.ToTable("Loans");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.LoanNumber)
                .IsUnique();

            entity.HasIndex(e => e.ApplicantEmail);

            entity.HasIndex(e => e.Status);

            entity.HasIndex(e => e.ApplicationDate);

            entity.Property(e => e.LoanNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ApplicantName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ApplicantEmail)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ApplicantPhone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.LoanAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.InterestRate)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            entity.Property(e => e.MonthlyPayment)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Purpose)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);
        });

        // Seed some initial data
        modelBuilder.Entity<Loan>().HasData(
            new Loan
            {
                Id = 1,
                LoanNumber = "LN-2024-000001",
                ApplicantName = "John Doe",
                ApplicantEmail = "john.doe@example.com",
                ApplicantPhone = "+1-555-0101",
                LoanAmount = 50000.00m,
                LoanTermMonths = 60,
                InterestRate = 7.5m,
                LoanType = LoanType.Personal,
                Status = LoanStatus.Approved,
                Purpose = "Home renovation and improvement project",
                MonthlyPayment = 1001.45m,
                ApplicationDate = DateTime.UtcNow.AddDays(-30),
                ApprovalDate = DateTime.UtcNow.AddDays(-15),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                CreatedBy = "System"
            },
            new Loan
            {
                Id = 2,
                LoanNumber = "LN-2024-000002",
                ApplicantName = "Jane Smith",
                ApplicantEmail = "jane.smith@example.com",
                ApplicantPhone = "+1-555-0102",
                LoanAmount = 250000.00m,
                LoanTermMonths = 240,
                InterestRate = 6.25m,
                LoanType = LoanType.Home,
                Status = LoanStatus.UnderReview,
                Purpose = "Purchase of primary residence",
                ApplicationDate = DateTime.UtcNow.AddDays(-7),
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                CreatedBy = "System"
            },
            new Loan
            {
                Id = 3,
                LoanNumber = "LN-2024-000003",
                ApplicantName = "Bob Johnson",
                ApplicantEmail = "bob.johnson@example.com",
                ApplicantPhone = "+1-555-0103",
                LoanAmount = 35000.00m,
                LoanTermMonths = 72,
                InterestRate = 5.99m,
                LoanType = LoanType.Auto,
                Status = LoanStatus.Pending,
                Purpose = "Purchase of new vehicle",
                ApplicationDate = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedBy = "System"
            }
        );
    }
}
