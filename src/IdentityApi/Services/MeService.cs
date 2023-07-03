using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Data.Repositories;
using IdentityApi.Results;

namespace IdentityApi.Services;

public class MeService : IMeService
{
	private readonly IUserRepository _userRepo;
	private readonly IUserContext _userCtx;
	private readonly IPasswordService _passwordService;

	public MeService(IUserRepository userRepo, IUserContext userCtx, IPasswordService passwordService)
	{
		_userRepo = userRepo;
		_userCtx = userCtx;
		_passwordService = passwordService;
	}
	
	
	public Task<UserProfile> GetAsync(CancellationToken ct)
	{
		var userId = _userCtx.GetId();
		
		return _userRepo.GetProfileAsync(userId, ct);
	}

	public async Task<Result<UserProfile>> UpdateUsername(UsernameUpdate update, CancellationToken ct)
	{
		var id = _userCtx.GetId();
		var user = (await _userRepo.GetAsync(id, ct))!;
		
		string? key = null;
		string? error = null;
		
		if (update.Username == user.Username)
		{
			key = "username";
			error = "Username cannot be the same";
		}
		else if (!_passwordService.VerifyPassword(update.Password, user.PasswordHash))
		{
			key = "password";
			error = "Password is not correct";
		}
		
		if (error != null)
		{
			return new(){ Errors = new() { {key!, new[] { error }}}};
		}
		
		var userProfile = await _userRepo.ChangeUsername(id, update.Username, ct);
		
		return new()
		{
			Subject = userProfile,
			Succeeded = true,
		};
	}
}