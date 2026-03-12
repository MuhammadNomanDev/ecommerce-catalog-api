using EcommerceCatalog.Domain.Entities;
using EcommerceCatalog.Domain.Interfaces;
using EcommerceCatalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCatalog.Infrastructure.Repositories;

// ── PRODUCT REPOSITORY ───────────────────────────────────────────────────────

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Product>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await _context.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        => await _context.Products.CountAsync(p => p.IsActive, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        => await _context.Products.AddAsync(product, cancellationToken);

    public void Update(Product product) => _context.Products.Update(product);

    public void Delete(Product product) => _context.Products.Remove(product);
}

// ── UNIT OF WORK ─────────────────────────────────────────────────────────────

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Products = new ProductRepository(context);
    }

    public IProductRepository Products { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
