using Azure.Storage.Blobs;
using EcommerceCatalog.API.Middleware;
using EcommerceCatalog.Application.Interfaces;
using EcommerceCatalog.Application.Products.Commands;
using EcommerceCatalog.Application.Products.Validators;
using EcommerceCatalog.Domain.Interfaces;
using EcommerceCatalog.Infrastructure.Persistence;
using EcommerceCatalog.Infrastructure.Repositories;
using EcommerceCatalog.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── SERILOG ───────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{MachineName}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ── DATABASE ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── REPOSITORIES ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── AZURE BLOB STORAGE ────────────────────────────────────────────────────────
//builder.Services.AddSingleton(new BlobServiceClient(
//    builder.Configuration["Azure:BlobStorage:ConnectionString"]));

builder.Services.AddScoped<IBlobStorageService>(provider =>
{
    var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
    var containerName = builder.Configuration["Azure:BlobStorage:ContainerName"] ?? "product-images";
    return new BlobStorageService(blobServiceClient, containerName);
});

// ── MEDIATR + CQRS ────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(EcommerceCatalog.Application.Common.Behaviours.LoggingBehaviour<,>));
});

// ── FLUENT VALIDATION ─────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();

// ── CONTROLLERS + SWAGGER ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Commerce Catalog API",
        Version = "v1",
        Description = "Clean Architecture + CQRS + Azure — Portfolio Project"
    });
});

var app = builder.Build();

// ── MIDDLEWARE ────────────────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging(); // logs every HTTP request in one clean line

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// ── AUTO MIGRATE ON STARTUP ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

public partial class Program { } // needed for integration tests
