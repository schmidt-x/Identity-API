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

namespace IdentityApi.Services;

public class AuthService : IAuthService
{
	private readonly ISessionService _sessionService;
	private readonly TokenValidationParameters _tokenValidationParameters;
	private readonly ICodeGenerationService _codeService;
	private readonly JwtOptions _jwt;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork _uow;

	public AuthService(
		ISessionService sessionService,
		IOptions<JwtOptions> jwtOptions,
		TokenValidationParameters tokenValidationParameters,
		ICodeGenerationService codeService,
		IPasswordService passwordService,
		IUnitOfWork uow)
	{
		_uow = uow;
		_sessionService = sessionService;
		_tokenValidationParameters = tokenValidationParameters;
		_codeService = codeService;
		_jwt = jwtOptions.Value;
		_passwordService = passwordService;
	}
	
	
	public async Task<SessionResult> CreateSessionAsync(string email, CancellationToken ct)
	{
		if (await _uow.UserRepo.EmailExistsAsync(email, ct))
		{
			return SessionResultFail(ErrorKey.Email, $"Email address '{email}' is already taken");
		}
		
		string verificationCode = _codeService.Generate();
		var sessionId = Guid.NewGuid().ToString();
		
		var session = new EmailSession
		{
			EmailAddress = email,
			VerificationCode = verificationCode,
		};
		
		_sessionService.Create(sessionId, session, TimeSpan.FromMinutes(5));
		
		return new SessionResult
		{
			Succeeded = true,
			Id = sessionId,
			VerificationCode = verificationCode
		};
	}
	
	public ResultEmpty VerifyEmail(string sessionId, string verificationCode)
	{
		if (!_sessionService.TryGetValue<EmailSession>(sessionId, out var session))
		{
			return ResultEmptyFail(ErrorKey.Session, ErrorMessage.SessionNotFound);
		}
		
		if (session!.IsVerified)
		{
			return ResultEmptyFail(ErrorKey.Email, ErrorMessage.EmailAlreadyVerified);
		}
		
		if (session.VerificationCode != verificationCode)
		{
			var attempts = ++session.Attempts;
			var result = ResultEmptyFail(ErrorKey.Code, AttemptsErrors(attempts));
			
			if (attempts >= 3)
			{
				_sessionService.Remove(sessionId);
			}
			
			return result;
		}
		
		session.IsVerified = true;
		_sessionService.Update(sessionId, session, TimeSpan.FromMinutes(5));
		
		return ResultEmptySuccess();
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
		var passwordHash = _passwordService.HashPassword(registrationRequest.Password);
		
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
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(CancellationToken.None);
			
			throw;
		}
		
		_sessionService.Remove(sessionId);
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<TokenGenerationResult> GenerateTokensAsync(UserClaims user, CancellationToken ct)
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
			
			throw;
		}
		
		return new TokenGenerationResult
		{
			AccessToken = handler.WriteToken(securityToken),
			RefreshToken = refreshToken.Id
		};
	}
	
	public async Task<AuthenticationResult> AuthenticateAsync(UserLoginRequest loginRequest, CancellationToken ct)
	{
		var user = await _uow.UserRepo.GetAsync(loginRequest.Login, ct);
		
		if (user is null || !_passwordService.VerifyPassword(loginRequest.Password, user.PasswordHash))
		{
			return AuthResultFail(ErrorKey.Login, ErrorMessage.WrongLoginPassword);
		}
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshingRequest tokens, CancellationToken ct)
	{
		if (!Guid.TryParse(tokens.RefreshToken, out var refreshTokenId))
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.InvalidRefreshToken);
		}
		
		var principal = ValidateTokenExceptLifetime(tokens.AccessToken, out var securityToken);
		
		if (principal == null)
		{
			return AuthResultFail(ErrorKey.AccessToken, ErrorMessage.InvalidAccessToken);
		}
		
		var refreshToken = await _uow.TokenRepo.GetAsync(refreshTokenId, ct);
		
		if (refreshToken == null)
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.InvalidRefreshToken);
		}
		
		if (refreshToken.Invalidated)
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenInvalidated);
		}
		
		if (refreshToken.Used)
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenUsed);
		}
		
		if (refreshToken.ExpiresAt < DateTime.UtcNow.GetTotalSeconds())
		{
			return AuthResultFail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenExpired);
		}
		
		if (!refreshToken.Jti.Equals(Guid.Parse(securityToken!.Id)))
		{
			return AuthResultFail(ErrorKey.AccessToken, ErrorMessage.TokensNotMatch);
		}
		
		try
		{
			var jti = await _uow.TokenRepo.SetUsedAsync(refreshTokenId, ct);
			// _tokenBlacklist.Add(jti.ToString());
			await _uow.SaveChangesAsync(ct);
		}
		catch(Exception ex)
		{
			await _uow.UndoChangesAsync(ct);
			
			throw;
		}
		
		return AuthResultSuccess(refreshToken.UserId, principal.FindEmail(true)!);
	}
	
	
	private ClaimsPrincipal? ValidateTokenExceptLifetime(string token, out SecurityToken? securityToken)
	{
		var handler = new JwtSecurityTokenHandler();
		securityToken = default;
		
		try
		{
			_tokenValidationParameters.ValidateLifetime = false;
			var claimsPrincipal = handler.ValidateToken(token, _tokenValidationParameters, out var secToken);
			
			if (!IsValidSecurityAlgorithm(secToken))
				return null;
			
			securityToken = secToken;
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
	
	private static ResultEmpty ResultEmptyFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
	
	private static ResultEmpty ResultEmptySuccess() => new() { Succeeded = true };
		
	private static SessionResult SessionResultFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
		
	private static string[] AttemptsErrors(int attempts)
	{
		var leftAttempts = 3 - attempts;
		
		var error = (leftAttempts) switch
		{
			1 => "1 last attempt is left",
			2 => "2 more attempts are left",
			_ => "No attempts are left"
		};
		
		return new[] { "Wrong verification code", error };
	}
}
