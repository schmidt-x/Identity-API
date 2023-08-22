using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Factories;

namespace IdentityApi.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
	private readonly TransactionFactory _transactionFactory;

	public IUserRepository UserRepo { get; }
	public IRefreshTokenRepository TokenRepo { get; }
	
	public UnitOfWork(
		TransactionFactory transactionFactory,
		IRefreshTokenRepository tokenRepo,
		IUserRepository userRepo)
	{
		_transactionFactory = transactionFactory;
		TokenRepo = tokenRepo;
		UserRepo = userRepo;
	}
	
	
	public async Task SaveChangesAsync(CancellationToken ct = default)
	{
		if (!_transactionFactory.IsActive) return;
		
		var transaction = await _transactionFactory.GetTransactionAsync();
		
		await transaction.CommitAsync(ct);
	}
	
	public async Task UndoChangesAsync(CancellationToken ct = default)
	{
		if (!_transactionFactory.IsActive) return;
		
		var transaction = await _transactionFactory.GetTransactionAsync();
		
		await transaction.RollbackAsync(ct);
	}
	
	public async ValueTask DisposeAsync()
	{
		await _transactionFactory.DisposeAsync();
	}
	
	public void Dispose()
	{
		_transactionFactory.Dispose();
	}
}