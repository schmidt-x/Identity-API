namespace IdentityLibrary.Repositories;

public interface IRefreshTokenRepository
{
	Task SaveAsync(RefreshToken refreshToken);
	Task<RefreshToken?> GetAsync(Guid tokenId);
	Task SetUsedAsync(Guid tokenId);
	Task InvalidateAsync(Guid id);
	Task InvalidateAllAsync(Guid id);
}