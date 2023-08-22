using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.Responses;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IMeService
{
	Task<Me> GetAsync(CancellationToken ct);
	Task<Result<Me>> UpdateUsernameAsync(string newUsername, string password, CancellationToken ct);
	string CreateEmailUpdateSession();
	ResultEmpty VerifyEmail(string verificationCode);
	Task<Result<string>> CacheNewEmailAsync(string newEmail, CancellationToken ct);
	Task<Result<Me>> UpdateEmailAsync(string verificationCode, CancellationToken ct);
	Task<Result<Me>> UpdatePasswordAsync(string password, string newPassword, CancellationToken ct);
}