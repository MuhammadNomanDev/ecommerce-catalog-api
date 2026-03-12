# 🚀 Setup Guide — Step by Step

## Prerequisites
- .NET 8 SDK → https://dotnet.microsoft.com/download
- Docker Desktop → https://www.docker.com/products/docker-desktop
- Azure Account (free) → https://portal.azure.com
- GitHub Account

---

## Step 1 — Run Locally with Docker

```bash
# 1. Clone the repo
git clone https://github.com/YOUR_USERNAME/ecommerce-catalog-api
cd ecommerce-catalog-api

# 2. Start API + SQL Server together
docker-compose up --build

# 3. Open Swagger
http://localhost:5000/swagger
```

---

## Step 2 — Set Up Azure Blob Storage

1. Go to portal.azure.com
2. Create a **Storage Account**
3. Go to **Access Keys** → copy the connection string
4. Replace `YOUR_AZURE_BLOB_CONNECTION_STRING` in appsettings.json or set it as an environment variable

---

## Step 3 — Set Up GitHub Secrets (for CI/CD)

In your GitHub repo → Settings → Secrets → Actions, add:

| Secret Name | Value |
|-------------|-------|
| `DOCKERHUB_USERNAME` | Your Docker Hub username |
| `DOCKERHUB_TOKEN` | Docker Hub access token |
| `AZURE_APP_SERVICE_NAME` | Your Azure App Service name |
| `AZURE_PUBLISH_PROFILE` | Download from Azure App Service → Get Publish Profile |

---

## Step 4 — Deploy to Azure App Service

1. Create an **App Service** in Azure (Linux, Docker container)
2. Set the environment variables in Azure App Service → Configuration
3. Push to `main` branch → GitHub Actions will build, test, and deploy automatically

---

## Step 5 — Run Tests

```bash
dotnet test
```

---

## GitFlow Workflow

```bash
# Start a new feature
git checkout develop
git checkout -b feature/add-category-filter

# Work on feature... then merge back
git checkout develop
git merge feature/add-category-filter

# When ready for production
git checkout main
git merge develop
git tag v1.0.0
```
