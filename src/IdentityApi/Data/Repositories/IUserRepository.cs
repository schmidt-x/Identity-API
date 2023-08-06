using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Models;

namespace IdentityApi.Data.Repositories;

public interface IUserRepository
{
	Task<bool> EmailExistsAsync(string email, CancellationToken ct);
	Task<bool> UsernameExistsAsync(string username, CancellationToken ct);
	Task SaveAsync(User user, CancellationToken ct);
	Task<User?> GetAsync(string email, CancellationToken ct);
	Task<User?> GetAsync(Guid id, CancellationToken ct);
	Task<User> GetRequiredAsync(Guid id, CancellationToken ct);
	Task<UserProfile> GetProfileAsync(Guid id, CancellationToken ct);
	Task<string> GetRoleAsync(Guid id, CancellationToken ct);
	Task<string> GetPasswordHashAsync(Guid id, CancellationToken ct);
	Task<UserProfile> ChangeUsernameAsync(Guid id, string username, CancellationToken ct);
	Task<UserProfile> ChangeEmailAsync(Guid id, string email, CancellationToken ct);
	public Task<UserProfile> ChangePasswordAsync(Guid id, string password, CancellationToken ct);
}