using EcommerceCatalog.Application.Interfaces;
using EcommerceCatalog.Application.Products.DTOs;
using EcommerceCatalog.Domain.Entities;
using EcommerceCatalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcommerceCatalog.Application.Products.Commands;

// ── CREATE ──────────────────────────────────────────────────────────────────

public record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity)
    : IRequest<ProductDto>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Description, request.Price, request.StockQuantity);
        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }
}

// ── UPDATE ──────────────────────────────────────────────────────────────────

public record UpdateProductCommand(Guid Id, string Name, string Description, decimal Price, int StockQuantity)
    : IRequest<ProductDto>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        product.Update(request.Name, request.Description, request.Price, request.StockQuantity);
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }
}

// ── DELETE ──────────────────────────────────────────────────────────────────

public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Deletion is a significant business event — worth logging directly
        // even though LoggingBehaviour already logs all commands
        _logger.LogWarning(
            "Product permanently deleted - ID: {ProductId}, Name: {ProductName}",
            product.Id,
            product.Name);
    }
}

// ── UPLOAD IMAGE ─────────────────────────────────────────────────────────────

public record UploadProductImageCommand(Guid ProductId, Stream ImageStream, string FileName, string ContentType)
    : IRequest<string>;

public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobStorageService _blobService;

    public UploadProductImageCommandHandler(IUnitOfWork unitOfWork, IBlobStorageService blobService)
    {
        _unitOfWork = unitOfWork;
        _blobService = blobService;
    }

    public async Task<string> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        var imageUrl = await _blobService.UploadAsync(request.ImageStream, request.FileName, request.ContentType, cancellationToken);
        product.SetImageUrl(imageUrl);
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return imageUrl;
    }
}

// ── MAPPING EXTENSION ────────────────────────────────────────────────────────

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.StockQuantity,
        p.ImageUrl, p.IsActive, p.CreatedAt, p.UpdatedAt);
}