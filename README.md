# Loan Application API

A .NET 8 Web API for managing loan applications with full CRUD operations, designed for deployment on OpenShift.

## Features

- **CRUD Operations**: Create, Read, Update, Delete loan applications
- **Pagination**: Paginated listing of loans
- **Filtering**: Filter by loan status and type
- **Search**: Search loans by applicant name, email, or loan number
- **Statistics**: Get loan statistics dashboard
- **Health Checks**: Kubernetes/OpenShift compatible health endpoints
- **Swagger/OpenAPI**: Interactive API documentation

## API Endpoints

### Loans

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/loans` | Get all loans (paginated) |
| GET | `/api/loans/{id}` | Get loan by ID |
| GET | `/api/loans/number/{loanNumber}` | Get loan by loan number |
| POST | `/api/loans` | Create new loan |
| PUT | `/api/loans/{id}` | Update loan |
| PATCH | `/api/loans/{id}/status` | Update loan status |
| DELETE | `/api/loans/{id}` | Delete loan |
| GET | `/api/loans/search?searchTerm=` | Search loans |
| GET | `/api/loans/statistics` | Get statistics |

### Health Checks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/health` | Detailed health check |
| GET | `/api/health/live` | Liveness probe |
| GET | `/api/health/ready` | Readiness probe |
| GET | `/api/health/startup` | Startup probe |

## Loan Types

- Personal
- Home
- Auto
- Business
- Education
- Medical

## Loan Statuses

- Pending
- UnderReview
- Approved
- Rejected
- Disbursed
- Closed
- Defaulted

## Running Locally

```bash
# Navigate to API project
cd LoanApplication.API

# Restore packages
dotnet restore

# Run the application
dotnet run

# Access Swagger UI
open http://localhost:8080/swagger
```

## Docker

```bash
# Build image
docker build -t loan-application-api .

# Run container
docker run -p 8080:8080 loan-application-api
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | Production |
| `ASPNETCORE_URLS` | Application URLs | http://+:8080 |
| `ConnectionStrings__DefaultConnection` | Database connection string | In-Memory |

## OpenShift Deployment

See the GitHub Actions workflows for automated deployment:

1. `01-openshift-cluster-setup.yml` - Set up ROSA cluster
2. `02-openshift-namespace-setup.yml` - Create namespace
3. `04-build-dotnet-app.yml` - Build and push Docker image
4. `05-deploy-dotnet-app.yml` - Deploy to OpenShift

## License

MIT License
