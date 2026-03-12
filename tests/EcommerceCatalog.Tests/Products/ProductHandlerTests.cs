using EcommerceCatalog.Application.Products.Commands;
using EcommerceCatalog.Application.Products.Queries;
using EcommerceCatalog.Domain.Entities;
using EcommerceCatalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EcommerceCatalog.Tests.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockRepo.Object);
        _handler = new CreateProductCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsProductDto()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", "A great product", 9.99m, 100);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(9.99m, result.Price);
        Assert.Equal(100, result.StockQuantity);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChanges()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", "Description", 9.99m, 10);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsAddAsync()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", "Description", 9.99m, 10);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockRepo.Object);
        _handler = new GetProductByIdQueryHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ReturnsDto()
    {
        // Arrange
        var product = Product.Create("Laptop", "A fast laptop", 999.99m, 5);
        _mockRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        // Assert
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Laptop", result.Name);
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockRepo.Object);

        var mockLogger = new Mock<ILogger<DeleteProductCommandHandler>>();
        _handler = new DeleteProductCommandHandler(_mockUnitOfWork.Object, mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ExistingProduct_DeletesAndSaves()
    {
        // Arrange
        var product = Product.Create("Old Product", "Description", 10m, 1);
        _mockRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.Delete(It.IsAny<Product>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
