using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Data.Repositories;
using IdentityApi.Domain.Models;
using IdentityApi.Contracts.Responses;
using IdentityApi.Domain.Constants;
using IdentityApi.Extensions;
using IdentityApi.Results;
using Serilog;

namespace IdentityApi.Services;

public class MeService : IMeService
{
	private readonly IUserContext _userCtx;
	private readonly IPasswordHasher _passwordHasher;
	private readonly ICodeGenerationService _codeService;
	private readonly ISessionService _sessionService;
	private readonly IJwtService _jwtService;
	private readonly IUnitOfWork _uow;
	private readonly ILogger _logger;
	private readonly ITokenBlacklist _tokenBlacklist;
	

	public MeService(
		IUserContext userCtx, 
		IPasswordHasher passwordHasher,
		ICodeGenerationService codeService, 
		ISessionService sessionService,
		IJwtService jwtService,
		IUnitOfWork uow,
		ILogger logger,
		ITokenBlacklist tokenBlacklist)
	{
		_userCtx = userCtx;
		_passwordHasher = passwordHasher;
		_codeService = codeService;
		_sessionService = sessionService;
		_jwtService = jwtService;
		_uow = uow;
		_logger = logger;
		_tokenBlacklist = tokenBlacklist;
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
		
		var userId = _userCtx.GetId();
		var user = await _uow.UserRepo.GetRequiredAsync(userId, ct);
		
		if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
		{
			return ResultFail<Me>(ErrorKey.Password, ErrorMessage.WrongPassword);
		}
		
		UserProfile profile;
		try
		{
			profile = await _uow.UserRepo.UpdateUsernameAsync(userId, newUsername, ct);
			await _uow.SaveChangesAsync(ct);
			
			_logger.Information(
				"Username is updated. New username: {newUsename}, old username: {oldUsername}, user: {userId}",
				newUsername, user.Username, userId);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			_logger.Error(ex, "Updating username: {errorMessage}. User: {userId}", ex.Message, user.Id);
			
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
		
		var userId = _userCtx.GetId();
		_sessionService.Create(userId.ToString(), session, TimeSpan.FromMinutes(5));
		
		_logger.Information("Email-update session is created. User: {userId}", userId);
		
		return verificationCode;
	}
	
	public async Task<Result<string>> CacheNewEmailAsync(string newEmail, CancellationToken ct)
	{
		var userId = _userCtx.GetId();
		var userIdAsString = userId.ToString();
		
		if (!_sessionService.TryGetValue<EmailSession>(userIdAsString, out var session))
		{
			return ResultFail<string>(ErrorKey.Session, ErrorMessage.SessionNotFound);
		}
		
		if (session!.IsVerified == false)
		{
			return ResultFail<string>(ErrorKey.Email, ErrorMessage.OldEmailNotVerified);
		}
		
		if (_userCtx.GetEmail() == newEmail)
		{
			return ResultFail<string>(ErrorKey.Email, ErrorMessage.EmailsEqual);
		}
		
		if (await _uow.UserRepo.EmailExistsAsync(newEmail, ct))
		{
			return ResultFail<string>(ErrorKey.Email, $"Email address '{newEmail}' is already taken");
		}
		
		var verificationCode = _codeService.Generate();
		
		session.EmailAddress = newEmail;
		session.VerificationCode = verificationCode;
		session.Attempts = 0;
		
		_sessionService.Update(userIdAsString, session, TimeSpan.FromMinutes(5));
		
		_logger.Information("New email address is cached. New email: {newEmail}, user: {userId}", newEmail, userId);
		
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
			
			var secondsLeft = _jwtService.GetSecondsLeft(_userCtx.GetExp());
			_tokenBlacklist.Add(currentJti.ToString(), TimeSpan.FromSeconds(secondsLeft));
			
			await _uow.SaveChangesAsync(ct);
			
			_logger.Information(
				"Email address is udpated. New email: {newEmail}, old email: {oldEmail}, user: {userId}",
				session.EmailAddress, _userCtx.GetEmail(), userId);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			_logger.Error(ex, "Updating email address: {errorMessage}. User: {userId}", ex.Message, userId);
			
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
		
		if (!_passwordHasher.VerifyPassword(password, passwordHash))
		{
			return ResultFail<Me>(ErrorKey.Password, ErrorMessage.WrongPassword);
		}
		
		var newJti = Guid.NewGuid();
		var newPasswordHash = _passwordHasher.HashPassword(newPassword);
		UserProfile profile;
		
		try
		{
			profile = await _uow.UserRepo.UpdatePasswordAsync(userId, newPasswordHash, ct);
			
			var jtis = await _uow.TokenRepo.InvalidateAllAsync(userId, ct);
			
			_tokenBlacklist.AddRange(jtis.Select(x => x.ToString()), _jwtService.TotalExpirationTime);
			
			// make valid the one (refresh token), that is a pair of currently authenticated access token,
			// so the user could still use it on token-refreshing
			await _uow.TokenRepo.UpdateJtiAndSetValidAsync(_userCtx.GetJti(), newJti, ct);
			
			await _uow.SaveChangesAsync(ct);
			
			_logger.Information("Password is updated. User: {userId}", userId);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			_logger.Error(ex, "Updating password: {errorMessage}. User: {userId}", ex.Message, userId);
			
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
	
	public async Task LogOutAsync(CancellationToken ct)
	{
		var userId = _userCtx.GetId();
		
		try
		{
			var jtis = await _uow.TokenRepo.InvalidateAllAsync(userId, ct);
			_tokenBlacklist.AddRange(jtis.Select(x => x.ToString()), _jwtService.TotalExpirationTime);
			
			_logger.Information("User has logged out. User: {userId}", userId);
			
			await _uow.SaveChangesAsync(ct);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(ct);
			_logger.Error(ex, "Logging out a user: {errorMessage}. User: {userId}", ex.Message, userId);
			
			throw;
		}
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