namespace EcommerceCatalog.Application.Products.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string? ImageUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);

public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
};
