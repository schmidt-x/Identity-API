using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Enums;
using IdentityApi.Models;

namespace IdentityApi.Data.Repositories;

public interface IUserRepository
{
	Task<bool> ExistsAsync<T>(T arg, Column column, CancellationToken ct = default);
	Task SaveAsync(User user, CancellationToken ct = default);
	Task<User?> GetAsync<T>(T arg, Column column, CancellationToken ct = default);
	Task<string> GetEmailAsync(Guid userId, CancellationToken ct = default);
}