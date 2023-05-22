using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Enums;
using IdentityApi.Models;
using IdentityApi.Results;

namespace IdentityApi.Data.Repositories;

public interface IUserRepository
{
	Task<bool> ExistsAsync<T>(T arg, Column column, CancellationToken ct = default);
	Task SaveAsync(User user, CancellationToken ct = default);
	Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
	Task<UserClaims?> GetClaimsAsync(Guid userId, CancellationToken ct = default);
}