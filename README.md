# 🛒 E-Commerce Product Catalog API

A production-ready REST API built with **ASP.NET Core 8**, following **Clean Architecture** and **CQRS** principles. Includes full CI/CD pipeline, Docker support, and Azure cloud integration.

---

## 🏗️ Architecture

This project follows **Clean Architecture** with 4 layers:

```
EcommerceCatalog.Domain          → Entities, Interfaces, Business Rules (no dependencies)
EcommerceCatalog.Application     → CQRS Handlers, DTOs, Validation (depends on Domain only)
EcommerceCatalog.Infrastructure  → EF Core, SQL Server, Azure Blob (depends on Application)
EcommerceCatalog.API             → Controllers, Middleware, DI setup (depends on all)
```

### Why Clean Architecture?
- Business logic is completely isolated from infrastructure concerns
- Easy to swap databases or cloud providers without touching business logic
- Each layer is independently testable
- Follows Dependency Inversion Principle (SOLID)

### Why CQRS with MediatR?
- Commands (write) and Queries (read) are separated — easier to optimise independently
- Controllers stay thin — they just dispatch requests via MediatR
- Each handler has a single responsibility — easier to test and maintain

---

## 🚀 Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| CQRS | MediatR |
| Validation | FluentValidation |
| File Storage | Azure Blob Storage |
| Testing | xUnit + Moq |
| Containerisation | Docker + docker-compose |
| CI/CD | GitHub Actions |
| Deployment | Azure App Service |
| API Docs | Swagger / OpenAPI |
| Logging | Serilog |

---

## 📁 Project Structure

```
EcommerceCatalogApi/
├── src/
│   ├── EcommerceCatalog.Domain/
│   │   ├── Entities/              # Product, Category
│   │   ├── Interfaces/            # IProductRepository, IUnitOfWork
│   │   └── Common/                # BaseEntity
│   ├── EcommerceCatalog.Application/
│   │   ├── Products/
│   │   │   ├── Commands/          # CreateProduct, UpdateProduct, DeleteProduct
│   │   │   └── Queries/           # GetProducts, GetProductById
│   │   ├── DTOs/                  # ProductDto, CreateProductDto
│   │   └── Interfaces/            # IBlobStorageService
│   ├── EcommerceCatalog.Infrastructure/
│   │   ├── Persistence/           # AppDbContext, EF Core config
│   │   ├── Repositories/          # ProductRepository, UnitOfWork
│   │   └── Services/              # BlobStorageService
│   └── EcommerceCatalog.API/
│       ├── Controllers/           # ProductsController
│       ├── Middleware/            # GlobalExceptionMiddleware
│       └── Extensions/            # ServiceCollectionExtensions
├── tests/
│   └── EcommerceCatalog.Tests/
│       └── Products/              # Handler unit tests
├── .github/
│   └── workflows/
│       └── ci-cd.yml              # Build → Test → Deploy pipeline
├── docker-compose.yml
├── Dockerfile
└── README.md
```

---

## 🐳 Running Locally with Docker

```bash
# Clone the repo
git clone https://github.com/YOUR_USERNAME/ecommerce-catalog-api.git
cd ecommerce-catalog-api

# Run with Docker Compose (API + SQL Server)
docker-compose up --build

# API will be available at:
http://localhost:5000/swagger
```

---

## 🔑 Environment Variables

```env
ConnectionStrings__DefaultConnection=Server=...;Database=EcommerceCatalog;...
Azure__BlobStorage__ConnectionString=DefaultEndpointsProtocol=...
Azure__BlobStorage__ContainerName=product-images
```

---

## 📡 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products (paginated) |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create new product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| POST | `/api/products/{id}/image` | Upload product image to Azure Blob |

---

## ✅ Running Tests

```bash
dotnet test
```

Tests cover:
- `CreateProductCommandHandler` — validates creation logic
- `GetProductsQueryHandler` — validates query and mapping
- `GetProductByIdQueryHandler` — validates not found handling

---

## 🔄 CI/CD Pipeline

GitHub Actions pipeline (`.github/workflows/ci-cd.yml`) runs on every push to `main`:

1. ✅ Restore & Build
2. ✅ Run all unit tests
3. ✅ Build Docker image
4. ✅ Push to Docker Hub
5. ✅ Deploy to Azure App Service

---

## 🌿 GitFlow Branch Strategy

```
main          → production code only
develop       → integration branch
feature/*     → new features (e.g. feature/add-pagination)
hotfix/*      → urgent production fixes
release/*     → release preparation
```
