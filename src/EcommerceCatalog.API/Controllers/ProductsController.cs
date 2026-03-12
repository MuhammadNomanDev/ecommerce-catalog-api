using EcommerceCatalog.Application.Products.Commands;
using EcommerceCatalog.Application.Products.DTOs;
using EcommerceCatalog.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all products (paginated)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetProductsQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a product by ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new product</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateProductCommand(dto.Name, dto.Description, dto.Price, dto.StockQuantity),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing product</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateProductCommand(id, dto.Name, dto.Description, dto.Price, dto.StockQuantity),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Delete a product</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Upload product image to Azure Blob Storage</summary>
    [HttpPost("{id:guid}/image")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0) return BadRequest("File is empty.");

        using var stream = file.OpenReadStream();
        var imageUrl = await _mediator.Send(
            new UploadProductImageCommand(id, stream, file.FileName, file.ContentType),
            cancellationToken);

        return Ok(new { imageUrl });
    }
}
