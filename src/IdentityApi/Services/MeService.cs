using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Data.Repositories;

namespace IdentityApi.Services;

public class MeService : IMeService
{
	private readonly IUserRepository _userRepo;
	private readonly IUserContext _userCtx;

	public MeService(IUserRepository userRepo, IUserContext userCtx)
	{
		_userRepo = userRepo;
		_userCtx = userCtx;
	}
	
	public Task<UserProfile> GetAsync(CancellationToken ct)
	{
		var userId = _userCtx.GetId();
		
		// TODO Should we care about null, if the user is authenticated anyway? 
		return _userRepo.GetProfileAsync(userId, ct)!;
	}
	
}