using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Models;

namespace IdentityApi.Data.Repositories;

public interface IRefreshTokenRepository
{
	Task SaveAsync(RefreshToken refreshToken, CancellationToken ct);
	Task<RefreshToken?> GetAsync(Guid tokenId, CancellationToken ct);
	Task SetUsedAsync(Guid tokenId, CancellationToken ct);
	Task<IEnumerable<string>> InvalidateAllAsync(Guid userId, CancellationToken ct);
	Task UpdateJtiAsync(Guid oldJti, Guid newJti, CancellationToken ct);
	Task UpdateJtiAndSetValidAsync(Guid oldJti, Guid newJti, CancellationToken ct);
}