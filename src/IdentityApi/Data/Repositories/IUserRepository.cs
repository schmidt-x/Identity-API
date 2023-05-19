using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Models;
using IdentityApi.Results;

namespace IdentityApi.Data.Repositories;

public interface IUserRepository
{
	Task<bool> EmailExistsAsync(string email, CancellationToken ct);
	Task<bool> UsernameExistsAsync(string username, CancellationToken ct);
	Task<UserExistsResult> UserExistsAsync(string email, string username, CancellationToken ct);
	Task SaveAsync(User user, CancellationToken ct);
	Task<User?> GetByEmailAsync(string email, CancellationToken ct);
	Task<UserClaims?> GetClaims(Guid userId, CancellationToken ct);
}