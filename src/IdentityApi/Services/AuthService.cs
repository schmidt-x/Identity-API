using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.Requests;
using IdentityApi.Options;
using IdentityApi.Data.Repositories;
using IdentityApi.Extensions;
using IdentityApi.Domain.Models;
using IdentityApi.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Services;

public class AuthService : IAuthService
{
	private readonly ICacheService _cacheService;
	private readonly TokenValidationParameters _tokenValidationParameters;
	private readonly ICodeGenerationService _codeService;
	private readonly JwtOptions _jwt;
	private readonly IPasswordService _passwordService;
	private readonly IUnitOfWork _uow;

	public AuthService(
		ICacheService cacheService,
		IOptions<JwtOptions> jwtOptions,
		TokenValidationParameters tokenValidationParameters,
		ICodeGenerationService codeService,
		IPasswordService passwordService,
		IUnitOfWork uow)
	{
		_uow = uow;
		_cacheService = cacheService;
		_tokenValidationParameters = tokenValidationParameters;
		_codeService = codeService;
		_jwt = jwtOptions.Value;
		_passwordService = passwordService;
	}
	
	
	public async Task<SessionResult> CreateSessionAsync(string email, CancellationToken ct)
	{
		if (await _uow.UserRepo.EmailExistsAsync(email, ct))
		{
			return SessionResultFail("email", $"Email address '{email}' is already taken");
		}
		
		string verificationCode = _codeService.Generate();
		var sessionId = Guid.NewGuid().ToString();
		
		var session = new UserSession
		{
			EmailAddress = email,
			VerificationCode = verificationCode,
		};
		
		_cacheService.Set(sessionId, session, TimeSpan.FromMinutes(5));
		
		return new SessionResult
		{
			Succeeded = true,
			Id = sessionId,
			VerificationCode = verificationCode
		};
	}
	
	public ResultEmpty VerifyEmail(string sessionId, string verificationCode)
	{
		if (!_cacheService.TryGetValue<UserSession>(sessionId, out var session))
		{
			return ResultEmptyFail("session", "No session was found");
		}
		
		if (session!.IsVerified)
		{
			return ResultEmptyFail("session", "Email address is already verified");
		} 
		
		if (session.VerificationCode != verificationCode)
		{
			var attempts = ++session.Attempts;
			var result = ResultEmptyFail("code", AttemptsErrors(attempts));
			
			if (attempts >= 3)
			{
				_cacheService.Remove(sessionId);
			}
			
			return result;
		}
		
		session.IsVerified = true;
		_cacheService.Update(sessionId, session, TimeSpan.FromMinutes(5));
		
		return ResultEmptySuccess();
	}
	
	public async Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistrationRequest registrationRequest, CancellationToken ct)
	{
		if (!_cacheService.TryGetValue<UserSession>(sessionId, out var session))
		{
			return AuthResultFail("session", "No session was found");
		}
		
		if (session!.IsVerified == false)
		{
			return AuthResultFail("email", "Email address is not verified");
		}
		
		if (await _uow.UserRepo.UsernameExistsAsync(registrationRequest.Username, ct))
		{
			return AuthResultFail("username", $"Username '{registrationRequest.Username}' is already taken");
		}
		
		var timeNow = DateTime.UtcNow;
		var user = new User
		{
			Id = Guid.NewGuid(),
			Username = registrationRequest.Username,
			PasswordHash = _passwordService.HashPassword(registrationRequest.Password),
			CreatedAt = timeNow,
			UpdatedAt = timeNow,
			Email = session.EmailAddress,
			Role = "user"
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
		
		_cacheService.Remove(sessionId);
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<TokenGenerationResult> GenerateTokensAsync(UserClaims user, CancellationToken ct)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var handler = new JwtSecurityTokenHandler();
		
		var identity = new ClaimsIdentity(new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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
			Jti = securityToken.Id,
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
			return AuthResultFail("login", "Incorrect login/password");
		}
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshingRequest tokens, CancellationToken ct)
	{
		if (!Guid.TryParse(tokens.RefreshToken, out var refreshTokenId))
		{
			return AuthResultFail("refresh_token", "Invalid refresh token");
		}
		
		var principal = ValidateTokenExceptLifetime(tokens.AccessToken, out var securityToken);
		
		if (principal == null)
		{
			return AuthResultFail("accessToken", "Invalid access token");
		}
		
		var refreshToken = await _uow.TokenRepo.GetAsync(refreshTokenId, ct);
		
		if (refreshToken == null)
		{
			return AuthResultFail("refreshToken", "Invalid refresh token");
		}
		
		if (refreshToken.Invalidated)
		{
			return AuthResultFail("refreshToken", "Refresh token is invalidated");
		}
		
		if (refreshToken.Used)
		{
			return AuthResultFail("refreshToken", "Refresh token has already been used");
		}
		
		if (refreshToken.ExpiresAt < DateTime.UtcNow.GetTotalSeconds())
		{
			return AuthResultFail("refreshToken", "Refresh token has been expired");
		}
		
		if (!refreshToken.Jti.Equals(securityToken!.Id, StringComparison.InvariantCultureIgnoreCase))
		{
			return AuthResultFail("accessToken", "Tokens do not match");
		}
		
		try
		{
			await _uow.TokenRepo.SetUsedAsync(refreshTokenId, ct);
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
