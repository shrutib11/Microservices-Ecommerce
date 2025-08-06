using Microsoft.EntityFrameworkCore.Storage;
using ProductService.Domain.Interfaces;

namespace ProductService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ProductDbContext _context;

    public UnitOfWork(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }
}
