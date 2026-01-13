using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LoanApplication.API.Data;
using LoanApplication.API.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// ===========================================
// Configure Services
// ===========================================

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString) || builder.Environment.IsDevelopment())
{
    // Use In-Memory database for development/testing
    builder.Services.AddDbContext<LoanDbContext>(options =>
        options.UseInMemoryDatabase("LoanApplicationDb"));
}
else
{
    // Use SQL Server for production
    builder.Services.AddDbContext<LoanDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
}

// Add Services
builder.Services.AddScoped<ILoanService, LoanService>();

// Add Controllers
builder.Services.AddControllers();

// Add API Explorer and Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Loan Application API",
        Version = "v1",
        Description = "A .NET Core Web API for managing loan applications with CRUD operations",
        Contact = new OpenApiContact
        {
            Name = "Loan Application Team",
            Email = "support@loanapp.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    c.EnableAnnotations();
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LoanDbContext>("database", tags: new[] { "ready" })
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "ready" });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===========================================
// Build Application
// ===========================================

var app = builder.Build();

// ===========================================
// Configure Middleware Pipeline
// ===========================================

// Enable Swagger in all environments for OpenShift
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loan Application API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Loan Application API Documentation";
});

// Enable HTTPS redirection only in production with proper certificates
if (!app.Environment.IsDevelopment())
{
    // Commented out for OpenShift as TLS termination is handled at the route level
    // app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthorization();

// Map health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Always return healthy for liveness
});

app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));

// ===========================================
// Initialize Database
// ===========================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LoanDbContext>();
        
        // Ensure database is created (for In-Memory DB)
        context.Database.EnsureCreated();
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}

// ===========================================
// Run Application
// ===========================================

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://+:{port}");

app.Logger.LogInformation("Starting Loan Application API on port {Port}", port);
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
