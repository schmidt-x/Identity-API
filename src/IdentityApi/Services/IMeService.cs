using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IMeService
{
	Task<UserProfile> GetAsync(CancellationToken ct);
	Task<Result<UserProfile>> UpdateUsernameAsync(UsernameUpdate user, CancellationToken ct);
	string CreateEmailUpdateSession();
	ResultEmpty VerifyOldEmail(string verificationCode);
	Task<Result<string>> CacheNewEmailAsync(string newEmail, CancellationToken ct);
	Task<Result<UserProfile>> VerifyAndUpdateNewEmailAsync(string verificationCode, CancellationToken ct);
}