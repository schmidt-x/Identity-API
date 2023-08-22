using System;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityApi.Data.Repositories;

public interface IUnitOfWork : IDisposable, IAsyncDisposable 
{
	IUserRepository UserRepo { get; }
	IRefreshTokenRepository TokenRepo { get; }
	
	Task SaveChangesAsync(CancellationToken ct = default);
	Task UndoChangesAsync(CancellationToken ct = default);
}