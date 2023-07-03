using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IMeService
{
	Task<UserProfile> GetAsync(CancellationToken ct);
	Task<Result<UserProfile>> UpdateUsername(UsernameUpdate user, CancellationToken ct);
}