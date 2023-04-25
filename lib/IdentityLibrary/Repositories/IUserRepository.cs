namespace IdentityLibrary.Repositories;

public interface IUserRepository
{
	Task<bool> EmailExistsAsync(string email);
	Task<bool> UsernameExistsAsync(string username);
	Task<UserExistsResult> UserExistsAsync(string email, string username);
	Task SaveAsync(User user);
	Task<User?> GetAsync(string username);
	Task<UserClaims?> GetClaims(Guid userId);
}