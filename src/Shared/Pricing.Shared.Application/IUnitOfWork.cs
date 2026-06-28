namespace Pricing.Shared.Application;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken ct = default);
}
