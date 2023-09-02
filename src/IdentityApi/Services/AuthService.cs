using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.Requests;
using IdentityApi.Data.Repositories;
using IdentityApi.Domain.Constants;
using IdentityApi.Extensions;
using IdentityApi.Domain.Models;
using IdentityApi.Results;
using Serilog;

namespace IdentityApi.Services;

public class AuthService : IAuthService
{
	private readonly ISessionService _sessionService;
	private readonly ICodeGenerator _codeGenerator;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IUnitOfWork _uow;
	private readonly ILogger _logger;
	private readonly ITokenBlacklist _tokenBlacklist;
	private readonly IJwtService _jwtService;

	public AuthService(
		ISessionService sessionService,
		ICodeGenerator codeGenerator,
		IPasswordHasher passwordHasher,
		IUnitOfWork uow,
		ILogger logger,
		ITokenBlacklist tokenBlacklist,
		IJwtService jwtService)
	{
		_uow = uow;
		_sessionService = sessionService;
		_codeGenerator = codeGenerator;
		_passwordHasher = passwordHasher;
		_logger = logger;
		_tokenBlacklist = tokenBlacklist;
		_jwtService = jwtService;
	}
	
	
	public async Task<SessionResult> CreateRegistrationSessionAsync(string email, CancellationToken ct)
	{
		if (await _uow.UserRepo.EmailExistsAsync(email, ct))
		{
			return SessionResult.Fail(ErrorKey.Email, $"Email address '{email}' is already taken");
		}
		
		var session = new EmailSession
		{
			EmailAddress = email,
			VerificationCode = _codeGenerator.Generate(),
		};
		
		var sessionId = Guid.NewGuid().ToString();
		_sessionService.Create(sessionId, session, TimeSpan.FromMinutes(5));
		
		return SessionResult.Success(sessionId, session.VerificationCode);
	}
	
	public async Task<AuthResult> RegisterAsync(string sessionId, UserRegistrationRequest registrationRequest, CancellationToken ct)
	{
		if (!_sessionService.TryGetValue<EmailSession>(sessionId, out var session))
		{
			return AuthResult.Fail(ErrorKey.Session, ErrorMessage.SessionNotFound);
		}
		
		if (session!.IsVerified == false)
		{
			return AuthResult.Fail(ErrorKey.Email, ErrorMessage.EmailNotVerified);
		}
		
		if (await _uow.UserRepo.UsernameExistsAsync(registrationRequest.Username, ct))
		{
			return AuthResult.Fail(ErrorKey.Username, $"Username '{registrationRequest.Username}' is already taken");
		}
		
		var timeNow = DateTime.UtcNow;
		var passwordHash = _passwordHasher.HashPassword(registrationRequest.Password);
		
		var user = new User
		{
			Id = Guid.NewGuid(),
			Username = registrationRequest.Username,
			PasswordHash = passwordHash,
			CreatedAt = timeNow,
			UpdatedAt = timeNow,
			Email = session.EmailAddress,
			Role = Role.User
		};
		
		try
		{
			await _uow.UserRepo.SaveAsync(user, ct);
			await _uow.SaveChangesAsync(ct);
			
			_logger.Information("User is successfully created. Id: {userId}, email: {email}", user.Id, user.Email);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			_logger.Error(ex, "Creating new user: {errorMessage}. User: {userId}", ex.Message, user.Id);
			
			throw;
		}
		
		_sessionService.Remove(sessionId);
		
		var userClaims = new UserClaims { Id = user.Id, Email = user.Email };
		
		return AuthResult.Success(userClaims);
	}
	
	public async Task<TokensResult> GenerateTokensAsync(UserClaims user, CancellationToken ct)
	{
		var tokens = _jwtService.GenerateTokens(user);
		
		try
		{
			await _uow.TokenRepo.SaveAsync(tokens.RefreshToken, ct);
			await _uow.SaveChangesAsync(ct);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			
			_logger.Error(
				ex,
				"Creating new refresh token: {errorMessage}. Token: {refreshTokenId}",
				ex.Message, tokens.RefreshToken.Id);
			
			throw;
		}
		
		return new TokensResult
		{
			AccessToken = tokens.AccessToken,
			RefreshToken = tokens.RefreshToken.Id.ToString()
		};
	}
	
	public async Task<AuthResult> AuthenticateAsync(UserLoginRequest loginRequest, CancellationToken ct)
	{
		var user = await _uow.UserRepo.GetAsync(loginRequest.Login, ct);
		
		if (user is null || !_passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash))
		{
			return AuthResult.Fail(ErrorKey.Login, ErrorMessage.WrongLoginPassword);
		}
		
		_logger.Information("User has logged in. User: {userId}", user.Id);
		
		var userClaims = new UserClaims { Id = user.Id, Email = user.Email };
		
		return AuthResult.Success(userClaims);
	}
	
	public async Task<AuthResult> ValidateTokensAsync(TokenRefreshingRequest tokens, CancellationToken ct)
	{
		if (!Guid.TryParse(tokens.RefreshToken, out var refreshTokenId))
		{
			return AuthResult.Fail(ErrorKey.RefreshToken, ErrorMessage.InvalidRefreshToken);
		}
		
		var principal = _jwtService.ValidateTokenExceptLifetime(tokens.AccessToken, out var jwtSecurityToken);
		
		if (principal == null)
		{
			return AuthResult.Fail(ErrorKey.AccessToken, ErrorMessage.InvalidAccessToken);
		}
		
		var refreshToken = await _uow.TokenRepo.GetAsync(refreshTokenId, ct);
		
		if (refreshToken == null)
		{
			return AuthResult.Fail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenNotFound);
		}
		
		var result = _jwtService.ValidateRefreshToken(refreshToken, Guid.Parse(jwtSecurityToken!.Id));
		
		if (!result.Succeeded)
		{
			return AuthResult.Fail(result.Errors);
		}
		
		try
		{
			await _uow.TokenRepo.SetUsedAsync(refreshTokenId, ct);
			
			if (!_jwtService.IsExpired(jwtSecurityToken.ValidTo.GetTotalSeconds(), out var secondsLeft))
			{
				_tokenBlacklist.Add(jwtSecurityToken.Id, TimeSpan.FromSeconds(secondsLeft));
			}
			
			await _uow.SaveChangesAsync(ct);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			
			_logger.Error(
				ex,
				"Setting 'used' to refresh token: {errorMessage}. Token: {refreshTokenId}",
				ex.Message, refreshTokenId);
			
			throw;
		}
		
		var userClaims = new UserClaims
		{
			Id = refreshToken.UserId,
			Email = principal.FindEmail(true)!
		};
		
		return AuthResult.Success(userClaims);
	}
	
}
