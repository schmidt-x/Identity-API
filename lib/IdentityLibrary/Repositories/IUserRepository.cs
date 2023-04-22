namespace IdentityLibrary.Repositories;

public interface IUserRepository
{
	Task<bool> EmailExistsAsync(string email);
	Task SaveAsync(User user);
	Task<bool> UsernameExistsAsync(string username);
	Task<User?> GetAsync(string username);
	Task<UserClaims?> GetClaims(Guid userId);
}