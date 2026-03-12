using EcommerceCatalog.Application.Products.Commands;
using EcommerceCatalog.Application.Products.DTOs;
using EcommerceCatalog.Domain.Interfaces;
using MediatR;

namespace EcommerceCatalog.Application.Products.Queries;

// ── GET ALL (PAGINATED) ──────────────────────────────────────────────────────

public record GetProductsQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResult<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var total = await _unitOfWork.Products.GetTotalCountAsync(cancellationToken);
        return new PaginatedResult<ProductDto>(products.Select(p => p.ToDto()), total, request.Page, request.PageSize);
    }
}

// ── GET BY ID ────────────────────────────────────────────────────────────────

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found.");

        return product.ToDto();
    }
}
