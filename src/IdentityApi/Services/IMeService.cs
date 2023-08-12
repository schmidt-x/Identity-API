using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Responses;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IMeService
{
	Task<Me> GetAsync(CancellationToken ct);
	Task<Result<Me>> UpdateUsernameAsync(UsernameUpdate user, CancellationToken ct);
	string CreateEmailUpdateSession();
	ResultEmpty VerifyOldEmail(string verificationCode);
	Task<Result<string>> CacheNewEmailAsync(string newEmail, CancellationToken ct);
	Task<Result<Me>> UpdateEmailAsync(string verificationCode, CancellationToken ct);
	Task<Result<Me>> UpdatePasswordAsync(PasswordChangeRequest passwords, CancellationToken ct);
}