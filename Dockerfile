# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and restore (layer caching — only re-runs if .csproj files change)
COPY src/EcommerceCatalog.Domain/EcommerceCatalog.Domain.csproj src/EcommerceCatalog.Domain/
COPY src/EcommerceCatalog.Application/EcommerceCatalog.Application.csproj src/EcommerceCatalog.Application/
COPY src/EcommerceCatalog.Infrastructure/EcommerceCatalog.Infrastructure.csproj src/EcommerceCatalog.Infrastructure/
COPY src/EcommerceCatalog.API/EcommerceCatalog.API.csproj src/EcommerceCatalog.API/
COPY tests/EcommerceCatalog.Tests/EcommerceCatalog.Tests.csproj tests/EcommerceCatalog.Tests/

RUN dotnet restore src/EcommerceCatalog.API/EcommerceCatalog.API.csproj

# Copy all source and build
COPY . .
RUN dotnet build src/EcommerceCatalog.API/EcommerceCatalog.API.csproj -c Release --no-restore

# ── Stage 2: Publish ──────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish src/EcommerceCatalog.API/EcommerceCatalog.API.csproj -c Release -o /app/publish --no-restore

# ── Stage 3: Runtime (smallest image) ────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user for security — good practice to mention in interviews
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EcommerceCatalog.API.dll"]
