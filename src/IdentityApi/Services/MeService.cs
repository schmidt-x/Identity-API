using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Data.Repositories;
using IdentityApi.Domain.Models;
using IdentityApi.Contracts.Responses;
using IdentityApi.Domain.Constants;
using IdentityApi.Results;

namespace IdentityApi.Services;

public class MeService : IMeService
{
	private readonly IUserContext _userCtx;
	private readonly IPasswordService _passwordService;
	private readonly ICodeGenerationService _codeService;
	private readonly ISessionService _sessionService;
	private readonly IJwtService _jwtService;
	private readonly IUnitOfWork _uow;

	public MeService(
		IUserContext userCtx, 
		IPasswordService passwordService,
		ICodeGenerationService codeService, 
		ISessionService sessionService,
		IJwtService jwtService,
		IUnitOfWork uow)
	{
		_userCtx = userCtx;
		_passwordService = passwordService;
		_codeService = codeService;
		_sessionService = sessionService;
		_jwtService = jwtService;
		_uow = uow;
	}
	
	
	public async Task<Me> GetAsync(CancellationToken ct)
	{
		var profile = await _uow.UserRepo.GetProfileAsync(_userCtx.GetId(), ct);
		
		return new Me
		{
			Username = profile.Username,
			Email = profile.Email,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt,
			Role = profile.Role,
			Token = _userCtx.GetToken()
		};
	}

	public async Task<Result<Me>> UpdateUsernameAsync(string newUsername, string password, CancellationToken ct)
	{
		if (await _uow.UserRepo.UsernameExistsAsync(newUsername, ct))
		{
			return ResultFail<Me>(ErrorKey.Username, $"Username '{newUsername}' is already taken");
		}
		
		var id = _userCtx.GetId();
		var user = await _uow.UserRepo.GetRequiredAsync(id, ct);
		
		if (!_passwordService.VerifyPassword(password, user.PasswordHash))
		{
			return ResultFail<Me>(ErrorKey.Password, ErrorMessage.WrongPassword);
		}
		
		UserProfile profile;
		try
		{
			profile = await _uow.UserRepo.UpdateUsernameAsync(id, newUsername, ct);
			await _uow.SaveChangesAsync(ct);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			
			throw;
		}
		
		var me = new Me
		{
			Username = profile.Username,
			Email = profile.Email,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt,
			Role = profile.Role,
			Token = _userCtx.GetToken()
		};
		
		return ResultSuccess(me);
	}

	public string CreateEmailUpdateSession()
	{
		var verificationCode = _codeService.Generate();
		
		var session = new EmailSession
		{
			EmailAddress = null!, // it's for new email, we don't need it yet
			VerificationCode = verificationCode,
			IsVerified = false,
			Attempts = 0
		};
		
		var userId = _userCtx.GetId().ToString();
		_sessionService.Create(userId, session, TimeSpan.FromMinutes(5));
		
		return verificationCode;
	}
	
	public async Task<Result<string>> CacheNewEmailAsync(string newEmail, CancellationToken ct)
	{
		var userId = _userCtx.GetId().ToString();
		
		if (!_sessionService.TryGetValue<EmailSession>(userId, out var session))
		{
			return ResultFail<string>(ErrorKey.Session, ErrorMessage.SessionNotFound);
		}
		
		if (session!.IsVerified == false)
		{
			return ResultFail<string>(ErrorKey.Email, ErrorMessage.OldEmailNotVerified);
		}
		
		if (await _uow.UserRepo.EmailExistsAsync(newEmail, ct))
		{
			return ResultFail<string>(ErrorKey.Email, $"Email address '{newEmail}' is already taken");
		}
		
		var verificationCode = _codeService.Generate();
		
		session.EmailAddress = newEmail;
		session.VerificationCode = verificationCode;
		session.Attempts = 0;
		
		_sessionService.Update(userId, session, TimeSpan.FromMinutes(5));
		
		return ResultSuccess(verificationCode);
	}

	public async Task<Result<Me>> UpdateEmailAsync(string verificationCode, CancellationToken ct)
	{
		var userId = _userCtx.GetId();
		var userIdAsString = userId.ToString();
		
		if (!_sessionService.TryGetValue<EmailSession>(userIdAsString, out var session))
		{
			return ResultFail<Me>(ErrorKey.Session, ErrorMessage.SessionNotFound);
		}
		
		if (session!.IsVerified == false)
		{
			return ResultFail<Me>(ErrorKey.Email, ErrorMessage.OldEmailNotVerified);
		}
		
		if (string.IsNullOrWhiteSpace(session.EmailAddress))
		{
			return ResultFail<Me>(ErrorKey.Email, ErrorMessage.NewEmailRequired);
		}
		
		if (session.VerificationCode != verificationCode)
		{
			var attempts = ++session.Attempts;
			var result = ResultFail<Me>(ErrorKey.Code, AttemptsErrors(attempts));
			
			if (attempts >= 3)
			{
				_sessionService.Remove(userIdAsString);
			}
			
			return result;
		}
		
		UserProfile profile;
		var newJti = Guid.NewGuid();
		try
		{
			profile = await _uow.UserRepo.UpdateEmailAsync(userId, session.EmailAddress, ct);
			
			// Since email address is stored in Jwt, we will return a new Jwt with updated address in it.
			// The old one (its jti) should be black-listed
			// Also we will replace 'jti' in RefreshToken table to the new one (so that they match on token-refreshing)
			
			var currentJti = _userCtx.GetJti();
			await _uow.TokenRepo.UpdateJtiAsync(currentJti, newJti, ct);
			
			// _tokenBlacklist.Add(currentJti);
			
			await _uow.SaveChangesAsync(ct);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			
			throw;
		}
		
		var newToken = _jwtService.UpdateToken(_userCtx.GetToken(), newJti, profile.Email);
		
		var me = new Me
		{
			Username = profile.Username,
			Email = profile.Email,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt,
			Role = profile.Role,
			Token = newToken
		};
		
		_sessionService.Remove(userIdAsString);
		
		return ResultSuccess(me);
	}
	
	public async Task<Result<Me>> UpdatePasswordAsync(string password, string newPassword, CancellationToken ct)
	{
		var userId = _userCtx.GetId();
		var passwordHash = await _uow.UserRepo.GetPasswordHashAsync(userId, ct);
		
		if (!_passwordService.VerifyPassword(password, passwordHash))
		{
			return ResultFail<Me>("password", "Password is not correct");
		}
		
		var newPasswordHash = _passwordService.HashPassword(newPassword);
		
		UserProfile profile;
		var newJti = Guid.NewGuid();
		
		try
		{
			profile = await _uow.UserRepo.UpdatePasswordAsync(userId, newPasswordHash, ct);
			
			var jtis = await _uow.TokenRepo.InvalidateAllAsync(userId, ct);
			// _tokenBlacklist.AddRange(jtis.Select(x => x.ToString());
			
			// make valid the one (refresh token), that is a pair of currently authenticated access token,
			// so the user could still use it on token-refreshing
			await _uow.TokenRepo.UpdateJtiAndSetValidAsync(_userCtx.GetJti(), newJti, ct);
			
			await _uow.SaveChangesAsync(ct);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			
			throw;
		}
		
		var newToken = _jwtService.UpdateToken(_userCtx.GetToken(), newJti);
		
		var me = new Me
		{
			Username = profile.Username,
			Email = profile.Email,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt,
			Role = profile.Role,
			Token = newToken
		};
		
		return ResultSuccess(me);
	}
	
	
	private static string[] AttemptsErrors(int attempts)
	{
		var leftAttempts = 3 - attempts;
		
		var errorMessage = (leftAttempts) switch
		{
			1 => "1 last attempt is left",
			2 => "2 more attempts are left",
			_ => "No attempts are left"
		};
		
		return new[] { "Wrong verification code", errorMessage };
	}
	
	private static Result<T> ResultFail<T>(string key, params string[] errors) =>
		new() { Errors = new() {{ key, errors }}, Succeeded = false };
		
	private static Result<T> ResultSuccess<T>(T value) =>
		new() { Value = value , Succeeded = true };
	
}