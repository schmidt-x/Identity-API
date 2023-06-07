using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Services;

public interface IMeService
{
	Task<UserProfile> GetAsync(CancellationToken ct);
}