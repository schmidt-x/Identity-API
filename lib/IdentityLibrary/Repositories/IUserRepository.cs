namespace IdentityLibrary.Repositories;

public interface IUserRepository
{
	Task SaveAsync(User user);
	Task<ExistsResult> ExistsAsync(string username, string email);
	Task<User?> GetAsync(string username);
	Task<UserClaims?> GetClaims(Guid userId); 
}