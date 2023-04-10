namespace IdentityLibrary.Repositories;

public interface IRefreshTokenRepository
{
	Task SaveAsync(RefreshToken refreshToken);
	Task<RefreshToken?> GetAsync(string id);
	Task SetUsedAsync(Guid id);
	Task InvalidateAsync(Guid id);
}