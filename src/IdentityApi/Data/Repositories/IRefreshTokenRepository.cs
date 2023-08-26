using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Domain.Models;

namespace IdentityApi.Data.Repositories;

public interface IRefreshTokenRepository
{
	Task SaveAsync(RefreshToken refreshToken, CancellationToken ct);
	Task<RefreshToken?> GetAsync(Guid tokenId, CancellationToken ct);
	Task<Guid> SetUsedAsync(Guid tokenId, CancellationToken ct);
	Task<IEnumerable<Guid>> InvalidateAllAsync(Guid userId, CancellationToken ct);
	Task UpdateJtiAsync(Guid jti, Guid newJti, CancellationToken ct);
	Task UpdateJtiAndSetValidAsync(Guid jti, Guid newJti, CancellationToken ct);
}