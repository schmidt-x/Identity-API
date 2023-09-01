using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.Requests;
using IdentityApi.Options;
using IdentityApi.Data.Repositories;
using IdentityApi.Domain.Constants;
using IdentityApi.Extensions;
using IdentityApi.Domain.Models;
using IdentityApi.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace IdentityApi.Services;

public class AuthService : IAuthService
{
	private readonly ISessionService _sessionService;
	private readonly TokenValidationParameters _tokenValidationParameters;
	private readonly ICodeGenerationService _codeService;
	private readonly JwtOptions _jwt;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IUnitOfWork _uow;
	private readonly ILogger _logger;
	private readonly ITokenBlacklist _tokenBlacklist;
	private readonly IJwtService _jwtService;

	public AuthService(
		ISessionService sessionService,
		IOptions<JwtOptions> jwtOptions,
		TokenValidationParameters tokenValidationParameters,
		ICodeGenerationService codeService,
		IPasswordHasher passwordHasher,
		IUnitOfWork uow,
		ILogger logger,
		ITokenBlacklist tokenBlacklist,
		IJwtService jwtService)
	{
		_uow = uow;
		_sessionService = sessionService;
		_tokenValidationParameters = tokenValidationParameters;
		_codeService = codeService;
		_jwt = jwtOptions.Value;
		_passwordHasher = passwordHasher;
		_logger = logger;
		_tokenBlacklist = tokenBlacklist;
		_jwtService = jwtService;
	}
	
	
	public async Task<SessionResult> CreateRegistrationSessionAsync(string email, CancellationToken ct)
	{
		if (await _uow.UserRepo.EmailExistsAsync(email, ct))
		{
			return SessionResultFail(ErrorKey.Email, $"Email address '{email}' is already taken");
		}
		
		var session = new EmailSession
		{
			EmailAddress = email,
			VerificationCode = _codeService.Generate(),
		};
		
		var sessionId = Guid.NewGuid().ToString();
		_sessionService.Create(sessionId, session, TimeSpan.FromMinutes(5));
		
		return new SessionResult
		{
			Succeeded = true,
			Id = sessionId,
			VerificationCode = session.VerificationCode
		};
	}
	
	public async Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistrationRequest registrationRequest, CancellationToken ct)
	{
		if (!_sessionService.TryGetValue<EmailSession>(sessionId, out var session))
		{
			return AuthResultFail(ErrorKey.Session, ErrorMessage.SessionNotFound);
		}
		
		if (session!.IsVerified == false)
		{
			return AuthResultFail(ErrorKey.Email, ErrorMessage.EmailNotVerified);
		}
		
		if (await _uow.UserRepo.UsernameExistsAsync(registrationRequest.Username, ct))
		{
			return AuthResultFail(ErrorKey.Username, $"Username '{registrationRequest.Username}' is already taken");
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
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<TokensResult> GenerateTokensAsync(UserClaims user, CancellationToken ct)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var handler = new JwtSecurityTokenHandler();
		
		var jti = Guid.NewGuid();
		var identity = new ClaimsIdentity(new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
			new Claim(JwtRegisteredClaimNames.Email, user.Email)
		}, JwtBearerDefaults.AuthenticationScheme);
		
		var descriptor = new SecurityTokenDescriptor
		{
			Subject = identity,
			Audience = _jwt.Audience,
			Issuer = _jwt.Issuer,
			Expires = DateTime.UtcNow.Add(_jwt.AccessTokenLifeTime),
			SigningCredentials = credentials
		};
		
		var securityToken = handler.CreateToken(descriptor);
		
		var timeNow = DateTime.UtcNow;
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Jti = jti,
			CreatedAt = timeNow.GetTotalSeconds(),
			ExpiresAt = timeNow.Add(_jwt.RefreshTokenLifeTime).GetTotalSeconds(),
			UserId = user.Id,
			Invalidated = false,
			Used = false
		};
		
		try
		{
			await _uow.TokenRepo.SaveAsync(refreshToken, ct);
			await _uow.SaveChangesAsync(ct);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(ct);
			_logger.Error(
				ex,
				"Creating new refresh token: {errorMessage}. Token: {refreshTokenId}",
				ex.Message, refreshToken.Id);
			
			throw;
		}
		
		return new TokensResult
		{
			AccessToken = handler.WriteToken(securityToken),
			RefreshToken = refreshToken.Id.ToString()
		};
	}
	
	public async Task<AuthenticationResult> AuthenticateAsync(UserLoginRequest loginRequest, CancellationToken ct)
	{
		var user = await _uow.UserRepo.GetAsync(loginRequest.Login, ct);
		
		if (user is null || !_passwordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash))
		{
			return AuthResultFail(ErrorKey.Login, ErrorMessage.WrongLoginPassword);
		}
		
		_logger.Information("User has logged in. User: {userId}", user.Id);
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshingRequest tokens, CancellationToken ct)
	{
		if (!Guid.TryParse(tokens.RefreshToken, out var refreshTokenId))
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.InvalidRefreshToken);
		}
		
		var principal = ValidateTokenExceptLifetime(tokens.AccessToken, out var jwtSecurityToken);
		
		if (principal == null)
		{
			return AuthResultFail(ErrorKey.AccessToken, ErrorMessage.InvalidAccessToken);
		}
		
		var refreshToken = await _uow.TokenRepo.GetAsync(refreshTokenId, ct);
		
		if (refreshToken == null)
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenNotFound);
		}
		
		if (refreshToken.Invalidated)
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenInvalidated);
		}
		
		if (refreshToken.Used)
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenUsed);
		}
		
		var secondsNow = DateTime.UtcNow.GetTotalSeconds();
		
		if (refreshToken.ExpiresAt < secondsNow)
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenExpired);
		}
		
		if (!refreshToken.Jti.Equals(Guid.Parse(jwtSecurityToken!.Id)))
		{
			return AuthResultFail(ErrorKey.AccessToken, ErrorMessage.TokensNotMatch);
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
			await _uow.UndoChangesAsync(ct);
			_logger.Error(
				ex,
				"Setting 'used' to refresh token: {errorMessage}. Token: {refreshTokenId}",
				ex.Message, refreshTokenId);
			
			throw;
		}
		
		return AuthResultSuccess(refreshToken.UserId, principal.FindEmail(true)!);
	}
	
	
	private ClaimsPrincipal? ValidateTokenExceptLifetime(string token, out JwtSecurityToken? securityToken)
	{
		var handler = new JwtSecurityTokenHandler();
		securityToken = default;
		
		try
		{
			_tokenValidationParameters.ValidateLifetime = false;
			var claimsPrincipal = handler.ValidateToken(token, _tokenValidationParameters, out var secToken);
			
			if (!IsValidSecurityAlgorithm(secToken))
				return null;
			
			securityToken = secToken as JwtSecurityToken;
			return claimsPrincipal;
		}
		catch
		{
			return null;
		}
		finally
		{
			_tokenValidationParameters.ValidateLifetime = true;
		}
	}
	
	private static bool IsValidSecurityAlgorithm(SecurityToken token)
	{
		return token is JwtSecurityToken jwtSecToken
			&& jwtSecToken.Header.Alg.Equals(
			SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
	}
	
	
	private static AuthenticationResult AuthResultFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
	
	private static AuthenticationResult AuthResultSuccess(Guid userId, string email) =>
		new() { Succeeded = true, Claims = new() { Id = userId, Email = email } };
	
	private static SessionResult SessionResultFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
}
