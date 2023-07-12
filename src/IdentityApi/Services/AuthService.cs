using System;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Contracts.Options;
using IdentityApi.Data.Repositories;
using IdentityApi.Models;
using IdentityApi.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Services;

public class AuthService : IAuthService
{
	private readonly IUserRepository _userRepo;
	private readonly IRefreshTokenRepository _tokenRepo;
	private readonly ICacheService _cacheService;
	private readonly TokenValidationParameters _tokenValidationParameters;
	private readonly ICodeGenerationService _codeService;
	private readonly JwtOptions _jwt;
	private readonly IPasswordService _passwordService;

	public AuthService(
		IUserRepository userRepo, 
		IRefreshTokenRepository tokenRepo,
		ICacheService cacheService,
		IOptions<JwtOptions> jwtOptions,
		TokenValidationParameters tokenValidationParameters,
		ICodeGenerationService codeService,
		IPasswordService passwordService)
	{
		_userRepo = userRepo;
		_tokenRepo = tokenRepo;
		_cacheService = cacheService;
		_tokenValidationParameters = tokenValidationParameters;
		_codeService = codeService;
		_jwt = jwtOptions.Value;
		_passwordService = passwordService;
	}
	
	
	public async Task<SessionResult> CreateSessionAsync(string email, CancellationToken ct)
	{
		if (await _userRepo.EmailExistsAsync(email, ct))
		{
			return new SessionResult { Errors = new()
			{
				{ "email", new[] { $"Email address '{email}' is already taken" } }
			}};
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
	
	public ErrorsResult VerifyEmail(string sessionId, string verificationCode)
	{
		string key = "session";
		string[]? errors = null;
		
		if (!_cacheService.TryGetValue<UserSession>(sessionId, out var session))
		{
			errors = new[] { "No session was found" };
		}
		else if (session!.IsVerified)
		{
			errors = new[] { "Email address is already verified" };
		} 
		else if (session.Attempts >= 3)
		{
			errors = new[] { "No attmempts are left" };
		}
		else if (session.VerificationCode != verificationCode)
		{
			key = "code";
			var attempts = 3 - (++session.Attempts);
			
			var errorMessage = (attempts) switch
			{
				1 => "1 last attempt is left",
				2 => "2 more attempts are left",
				_ => "No attempts are left"
			};
			
			errors = new[] { "Wrong verification code", errorMessage };
		}
		
		if (errors != null)
		{
			return new ErrorsResult { Errors = new() { { key, errors } } };
		}
		
		session!.IsVerified = true;
		return new ErrorsResult { Succeeded = true };
	}
	
	public async Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistration userRegistration, CancellationToken ct)
	{
		if (!_cacheService.TryGetValue<UserSession>(sessionId, out var session))
		{
			return AuthResultFail("session", "No session was found");
		}
		
		if (session!.IsVerified == false)
		{
			return AuthResultFail("email", "Email address is not verified");
		}
		
		if (await _userRepo.UsernameExistsAsync(userRegistration.Username, ct))
		{
			return AuthResultFail("username", $"Username '{userRegistration.Username}' is already taken");
		}
		
		var timeNow = DateTime.UtcNow;
		var user = new User
		{
			Id = Guid.NewGuid(),
			Username = userRegistration.Username,
			PasswordHash = _passwordService.HashPassword(userRegistration.Password),
			CreatedAt = timeNow,
			UpdatedAt = timeNow,
			Email = session.EmailAddress,
			Role = "user"
		};
		
		try
		{
			await _userRepo.SaveAsync(user, ct);
		}
		catch(SqlException ex) when (ex.Number == 2627) // in case if 'Race condition' occurs
		{
			var error = GetSqlUQConstraintMessage(ex);

			error.value = error.key switch
			{
				"username" => string.Format(error.value, user.Username),
				"email" => string.Format(error.value, user.Email),
				_ => error.value
			};

			return AuthResultFail(error.key, error.value);
		}
		
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
		
		var timeNow = DateTimeOffset.UtcNow;
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Jti = securityToken.Id,
			CreatedAt = timeNow.ToUnixTimeSeconds(),
			ExpiresAt = timeNow.Add(_jwt.RefreshTokenLifeTime).ToUnixTimeSeconds(),
			UserId = user.Id,
			Invalidated = false,
			Used = false
		};
		
		await _tokenRepo.SaveAsync(refreshToken, ct);
		
		return new TokenGenerationResult
		{
			AccessToken = handler.WriteToken(securityToken),
			RefreshToken = refreshToken.Id
		};
	}
	
	public async Task<AuthenticationResult> AuthenticateAsync(UserLogin userLogin, CancellationToken ct)
	{
		var user = await _userRepo.GetAsync(userLogin.Login, ct);
		
		if (user == null || !_passwordService.VerifyPassword(userLogin.Password, user.PasswordHash))
		{
			return AuthResultFail("user", "Incorrect login/password");
		}
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshing tokens, CancellationToken ct)
	{
		if (!Guid.TryParse(tokens.RefreshToken, out var refreshTokenId))
		{
			return AuthResultFail("refreshToken", "Invalid refresh token");
		}
		
		var user = ValidateTokenExceptLifetime(tokens.AccessToken, out var securityToken);
		
		if (user == null)
		{
			return AuthResultFail("accessToken", "Invalid access token");
		}
		
		var refreshToken = await _tokenRepo.GetAsync(refreshTokenId, ct);
		
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
		
		if (refreshToken.ExpiresAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
		{
			return AuthResultFail("refreshToken", "Refresh token has been expired");
		}
		
		if (refreshToken.Jti != securityToken!.Id)
		{
			return AuthResultFail("accessToken", "Tokens do not match");
		}
		
		await _tokenRepo.SetUsedAsync(refreshTokenId, ct);
		var email = user.Claims.Single(x => x.Type == ClaimTypes.Email).Value;
		
		return AuthResultSuccess(refreshToken.UserId, email);
	}
	
	
	private static (string key, string value) GetSqlUQConstraintMessage(SqlException ex)
	{
		var message = ex.Message;
		const int startIndex = 36;
		var endIndex = message.IndexOf('.') - 1;
		
		var constraint = message.Substring(startIndex, endIndex - startIndex);
		
		// constraint name template is 'Constraint_Table_column'
		// for example 'UQ_User_email', 'UQ_User_username', etc.
		var parts = constraint.Split('_');
		var key = parts[2];
		
		if (key == "id")
			throw new ArgumentException($"Collision of guid. Constraint: '{constraint}'");
		
		var value = key switch
		{
			"email" => "Email address '{0}' is already taken",
			"username" => "Username '{0}' is already taken",
			_ => string.Empty 
		};
		
		return (key, value);
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
	
	
	private static AuthenticationResult AuthResultFail(string key, params string[] errors)
	{
		return new AuthenticationResult 
		{ 
			Succeeded = false,
			Errors = new() { { key, errors } } 
		};
	}
	private static AuthenticationResult AuthResultSuccess(Guid userId, string email)
	{
		return new AuthenticationResult
		{
			Succeeded = true,
			User = new()
			{
				Id = userId,
				Email = email
			}
		};
	}
}