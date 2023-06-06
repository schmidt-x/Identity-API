using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Models;

namespace IdentityApi.Data.Repositories;

public interface IUserRepository
{
	Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
	Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
	Task SaveAsync(User user, CancellationToken ct = default);
	Task<User?> GetAsync(string email, CancellationToken ct = default);
	Task<User?> GetAsync(Guid id, CancellationToken ct = default);
	Task<string?> GetRoleAsync(Guid id, CancellationToken ct = default);
}