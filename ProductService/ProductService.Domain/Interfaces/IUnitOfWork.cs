using Microsoft.EntityFrameworkCore.Storage;
namespace ProductService.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync();
}
