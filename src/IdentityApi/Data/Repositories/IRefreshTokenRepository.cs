using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Models;

namespace IdentityApi.Data.Repositories;

public interface IRefreshTokenRepository
{
	Task SaveAsync(RefreshToken refreshToken, CancellationToken ct);
	Task<RefreshToken?> GetAsync(Guid tokenId, CancellationToken ct);
	Task SetUsedAsync(Guid tokenId, CancellationToken ct);
	Task InvalidateAsync(Guid tokenId, CancellationToken ct);
	Task InvalidateAllAsync(Guid userId, CancellationToken ct);
}