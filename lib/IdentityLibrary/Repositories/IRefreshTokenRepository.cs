namespace IdentityLibrary.Repositories;

public interface IRefreshTokenRepository
{
	Task SaveAsync(RefreshToken refreshToken);
	Task<RefreshToken?> GetAsync(Guid id);
	Task SetUsedAsync(Guid id);
	Task InvalidateAsync(Guid id);
}